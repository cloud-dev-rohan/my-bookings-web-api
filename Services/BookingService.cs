using MyBookingsWebApi.Data;
using MyBookingsWebApi.Models;
using MyBookingsWebApi.Repository;


namespace MyBookingsWebApi.Services
{
    public interface IBookingService
    {
        Task<(bool Success, string Message, Guid? BookingId)> BookAsync(BookingRequest request);
        Task<(bool Success, string Message)> CancelAsync(Guid bookingId);
        Task<List<Booking>> GetAllBookingsByMemberIdAsync(Guid memberId);
        Task<List<Inventory>> GetTripDetailsAsync();
    }

    public class BookingService : IBookingService
    {
        private const int MAX_BOOKINGS = 2;
        private readonly IMemberRepository _memberRepo;
        private readonly IInventoryRepository _inventoryRepo;
        private readonly IBookingRepository _bookingRepo;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IMemberRepository memberRepo,
            IInventoryRepository inventoryRepo,
            IBookingRepository bookingRepo,
            AppDbContext dbContext,
            ILogger<BookingService> logger)
        {
            _memberRepo = memberRepo;
            _inventoryRepo = inventoryRepo;
            _bookingRepo = bookingRepo;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, Guid? BookingId)> BookAsync(BookingRequest request)
        {
            _logger.LogInformation("Starting booking process for MemberId: {MemberId}, TripId: {TripId}", request.MemberId, request.TripId);

            var member = await _memberRepo.GetByIdAsync(request.MemberId);
            if (member == null)
            {
                _logger.LogWarning("Booking failed: Member {MemberId} not found", request.MemberId);
                return (false, "Member not found", null);
            }

            var inventory = await _inventoryRepo.GetByIdAsync(request.TripId);
            if (inventory == null)
            {
                _logger.LogWarning("Booking failed: Inventory {TripId} not found", request.TripId);
                return (false, "Inventory not found", null);
            }

            if (inventory.ExpirationDate.CompareTo(DateTime.UtcNow) <= 0)
            {
                _logger.LogWarning("Booking failed: Inventory {TripId} expired", request.TripId);
                return (false, "Inventory duration expired", null);
            }

            if (member.BookingCount >= MAX_BOOKINGS)
            {
                _logger.LogWarning("Booking failed: Member {MemberId} exceeded max bookings", request.MemberId);
                return (false, "Member has reached maximum bookings", null);
            }

            if (inventory.RemainingCount <= 0)
            {
                _logger.LogWarning("Booking failed: No remaining inventory for {TripId}", request.TripId);
                return (false, "No inventory available", null);
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    MemberId = member.Id,
                    InventoryId = inventory.Id,
                    BookingDate = DateTime.UtcNow
                };

                await _bookingRepo.AddAsync(booking);
                member.BookingCount++;
                inventory.RemainingCount--;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Booking successful: BookingId {BookingId}", booking.Id);
                return (true, "Booking successful", booking.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Booking failed for MemberId {MemberId}, TripId {TripId}", request.MemberId, request.TripId);
                return (false, $"Booking failed: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message)> CancelAsync(Guid bookingId)
        {
            _logger.LogInformation("Starting cancellation process for BookingId: {BookingId}", bookingId);

            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
            {
                _logger.LogWarning("Cancellation failed: Booking {BookingId} not found", bookingId);
                return (false, "Booking not found");
            }

            var member = await _memberRepo.GetByIdAsync(booking.MemberId);
            var inventory = await _inventoryRepo.GetByIdAsync(booking.InventoryId);
            if (member == null || inventory == null)
            {
                _logger.LogError("Cancellation failed due to data inconsistency for BookingId {BookingId}", bookingId);
                return (false, "Data inconsistency");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _bookingRepo.RemoveAsync(booking);
                member.BookingCount = Math.Max(0, member.BookingCount - 1);
                inventory.RemainingCount++;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Cancellation successful for BookingId {BookingId}", bookingId);
                return (true, "Cancellation successful");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Cancellation failed for BookingId {BookingId}", bookingId);
                return (false, $"Cancellation failed: {ex.Message}");
            }
        }

        public Task<List<Booking>> GetAllBookingsByMemberIdAsync(Guid memberId)
        {
            _logger.LogInformation("Fetching all bookings for MemberId: {MemberId}", memberId);
            return _bookingRepo.GetAllBookingsByMemberIdAsync(memberId);
        }

        public Task<List<Inventory>> GetTripDetailsAsync()
        {
            _logger.LogInformation("Fetching all trip details");
            return _inventoryRepo.GetAllInventoriesAsync();
        }
    }
}
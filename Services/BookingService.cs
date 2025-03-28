namespace MyBookingsWebApi.Services
{
    using MyBookingsWebApi.Data;
    using MyBookingsWebApi.Models;
    using MyBookingsWebApi.Repository;
    using System;
    using System.Threading.Tasks;

    namespace MyBookingApp.Services
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
            // Define a Polly retry policy for transient faults.

            public BookingService(
                IMemberRepository memberRepo,
                IInventoryRepository inventoryRepo,
                IBookingRepository bookingRepo,
                AppDbContext dbContext)
            {
                _memberRepo = memberRepo;
                _inventoryRepo = inventoryRepo;
                _bookingRepo = bookingRepo;
                _dbContext = dbContext;
                
            }

            public async Task<(bool Success, string Message, Guid? BookingId)> BookAsync(BookingRequest request)
            {

                var member = await _memberRepo.GetByIdAsync(request.MemberId);
                if (member == null)
                    return (false, "Member not found", null);

                var inventory = await _inventoryRepo.GetByIdAsync(request.TripId);

                if (inventory == null)
                    return (false, "Inventory not found", null);

                if (inventory.ExpirationDate.CompareTo(DateTime.UtcNow)<=0)
                {
                    //Code to remove inventory
                    return (false, "Inventory duration expired", null);
                }

                if (member.BookingCount >= MAX_BOOKINGS)
                    return (false, "Member has reached maximum bookings", null);

                if (inventory.RemainingCount <= 0)
                    return (false, "No inventory available", null);

                // Begin an EF Core transaction
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
                    member.BookingCount += 1;
                    inventory.RemainingCount -= 1;

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Booking successful", booking.Id);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Booking failed: {ex.Message}", null);
                }

            }

            public async Task<(bool Success, string Message)> CancelAsync(Guid bookingId)
            {

                var booking = await _bookingRepo.GetByIdAsync(bookingId);
                if (booking == null)
                    return (false, "Booking not found");

                var member = await _memberRepo.GetByIdAsync(booking.MemberId);
                var inventory = await _inventoryRepo.GetByIdAsync(booking.InventoryId);
                if (member == null || inventory == null)
                    return (false, "Data inconsistency");

                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    await _bookingRepo.RemoveAsync(booking);
                    member.BookingCount = Math.Max(0, member.BookingCount - 1);
                    inventory.RemainingCount += 1;

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Cancellation successful");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Cancellation failed: {ex.Message}");
                }

            }

            public Task<List<Booking>> GetAllBookingsByMemberIdAsync(Guid memberId)
            {
                return _bookingRepo.GetAllBookingsByMemberIdAsync(memberId);
            }
            public Task<List<Inventory>> GetTripDetailsAsync()
            {
                return _inventoryRepo.GetAllInventoriesAsync();
            }
        }
    }

}

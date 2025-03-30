using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MyBookingsWebApi.Data;
using MyBookingsWebApi.Models;
using MyBookingsWebApi.Repository;
using MyBookingsWebApi.Services;
using Polly;

namespace MyBookingsWebApi.Tests
{
    [TestFixture]
    public class BookingServiceTests
    {
        private BookingService _bookingService;
        private Mock<IMemberRepository> _memberRepoMock;
        private Mock<IInventoryRepository> _inventoryRepoMock;
        private Mock<IBookingRepository> _bookingRepoMock;
        private AppDbContext _dbContext;
        private Mock<ILogger<BookingService>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            _memberRepoMock = new Mock<IMemberRepository>();
            _inventoryRepoMock = new Mock<IInventoryRepository>();
            _bookingRepoMock = new Mock<IBookingRepository>();
            _loggerMock = new Mock<ILogger<BookingService>>();

            var options = new DbContextOptionsBuilder<AppDbContext>()
               .UseInMemoryDatabase(databaseName: "TestDb")
               .Options;

            _dbContext = new AppDbContext(options);

            _bookingService = new BookingService(
                _memberRepoMock.Object,
                _inventoryRepoMock.Object,
                _bookingRepoMock.Object,
                _dbContext,
                _loggerMock.Object);
        }

        [Test]
        public async Task BookAsync_ShouldReturnFailure_WhenMemberNotFound()
        {
            // Arrange
            var request = new BookingRequest { MemberId = Guid.NewGuid(), TripId = Guid.NewGuid() };
            _memberRepoMock.Setup(repo => repo.GetByIdAsync(request.MemberId)).ReturnsAsync((Member)null);

            // Act
            var result = await _bookingService.BookAsync(request);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Member not found"));
        }

        [Test]
        public async Task BookAsync_ShouldReturnFailure_WhenInventoryNotFound()
        {
            // Arrange
            var request = new BookingRequest { MemberId = Guid.NewGuid(), TripId = Guid.NewGuid() };
            _memberRepoMock.Setup(repo => repo.GetByIdAsync(request.MemberId)).ReturnsAsync(new Member { Id = request.MemberId, BookingCount = 0 });
            _inventoryRepoMock.Setup(repo => repo.GetByIdAsync(request.TripId)).ReturnsAsync((Inventory)null);

            // Act
            var result = await _bookingService.BookAsync(request);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Inventory not found"));
        }

        [Test]
        public async Task BookAsync_ShouldReturnFailure_WhenInventoryExpired()
        {
            // Arrange
            var request = new BookingRequest { MemberId = Guid.NewGuid(), TripId = Guid.NewGuid() };
            _memberRepoMock.Setup(repo => repo.GetByIdAsync(request.MemberId)).ReturnsAsync(new Member { Id = request.MemberId, BookingCount = 0 });
            _inventoryRepoMock.Setup(repo => repo.GetByIdAsync(request.TripId)).ReturnsAsync(new Inventory { Id = request.TripId, ExpirationDate = DateTime.UtcNow.AddDays(-1) });

            // Act
            var result = await _bookingService.BookAsync(request);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Inventory duration expired"));
        }

        [Test]
        public async Task BookAsync_ShouldReturnSuccess_WhenBookingIsValid()
        {
            // Arrange
            var request = new BookingRequest { MemberId = Guid.NewGuid(), TripId = Guid.NewGuid() };
            var member = new Member { Id = request.MemberId, BookingCount = 1 };
            var inventory = new Inventory { Id = request.TripId, ExpirationDate = DateTime.UtcNow.AddDays(5), RemainingCount = 1 };

            _memberRepoMock.Setup(repo => repo.GetByIdAsync(request.MemberId)).ReturnsAsync(member);
            _inventoryRepoMock.Setup(repo => repo.GetByIdAsync(request.TripId)).ReturnsAsync(inventory);
            _bookingRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Booking>())).Returns(Task.CompletedTask);

            // Act
            var result = await _bookingService.BookAsync(request);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Booking successful"));
        }

        [Test]
        public async Task CancelAsync_ShouldReturnFailure_WhenBookingNotFound()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            _bookingRepoMock.Setup(repo => repo.GetByIdAsync(bookingId)).ReturnsAsync((Booking)null);

            // Act
            var result = await _bookingService.CancelAsync(bookingId);

            // Assert
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Booking not found"));
        }

        [Test]
        public async Task CancelAsync_ShouldReturnSuccess_WhenCancellationIsValid()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = new Booking { Id = bookingId, MemberId = Guid.NewGuid(), InventoryId = Guid.NewGuid() };
            var member = new Member { Id = booking.MemberId, BookingCount = 1 };
            var inventory = new Inventory { Id = booking.InventoryId, RemainingCount = 5 };

            _bookingRepoMock.Setup(repo => repo.GetByIdAsync(bookingId)).ReturnsAsync(booking);
            _memberRepoMock.Setup(repo => repo.GetByIdAsync(booking.MemberId)).ReturnsAsync(member);
            _inventoryRepoMock.Setup(repo => repo.GetByIdAsync(booking.InventoryId)).ReturnsAsync(inventory);
            _bookingRepoMock.Setup(repo => repo.RemoveAsync(booking)).Returns(Task.CompletedTask);

            // Act
            var result = await _bookingService.CancelAsync(bookingId);

            // Assert
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("Cancellation successful"));
        }

        [Test]
        public async Task GetAllBookingsByMemberIdAsync_ShouldReturnBookings()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var bookings = new List<Booking> { new Booking { Id = Guid.NewGuid(), MemberId = memberId } };
            _bookingRepoMock.Setup(repo => repo.GetAllBookingsByMemberIdAsync(memberId)).ReturnsAsync(bookings);

            // Act
            var result = await _bookingService.GetAllBookingsByMemberIdAsync(memberId);

            // Assert
            Assert.That(result, !Is.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }
        [TearDown]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using MyBookingsWebApi.Models;
using MyBookingsWebApi.Services;

namespace MyBookingsWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost("book")]
        public async Task<IActionResult> Book([FromBody] BookingRequest request)
        {
            var result = await _bookingService.BookAsync(request);
            if (result.Success)
                return Ok(new { message = result.Message, bookingId = result.BookingId });
            return BadRequest(new { message = result.Message });
        }

        [HttpDelete("cancel/{bookingId}")]
        public async Task<IActionResult> Cancel(Guid bookingId)
        {
            var result = await _bookingService.CancelAsync(bookingId);
            if (result.Success)
                return Ok(new { message = result.Message });
            return BadRequest(new { message = result.Message });
        }
        [HttpGet("bookings/{memberId}")]
        public async Task<IActionResult> GetAllBookings(Guid memberId)
        {
            var bookings = await _bookingService.GetAllBookingsByMemberIdAsync(memberId);
            return Ok(bookings);
        }

        [HttpGet("trip-details")]
        public async Task<IActionResult> GetTripDetails()
        {
            var trips = await _bookingService.GetTripDetailsAsync();
            return Ok(trips);
        }
    }
}

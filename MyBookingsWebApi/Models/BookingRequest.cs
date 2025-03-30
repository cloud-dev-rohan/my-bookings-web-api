namespace MyBookingsWebApi.Models
{
    public class BookingRequest
    {
        public Guid MemberId { get; set; }
        public Guid TripId { get; set; }
    }
}

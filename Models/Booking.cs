namespace MyBookingsWebApi.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid MemberId { get; set; }
        public Guid InventoryId { get; set; }
        public DateTime BookingDate { get; set; }
    }
}

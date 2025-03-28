namespace MyBookingsWebApi.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid MemberId { get; set; }
        public Guid InventoryId { get; set; }

        private DateTime _bookingDate;

        public DateTime BookingDate
        {
            get => _bookingDate;
            set => _bookingDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
    }
}

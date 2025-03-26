namespace MyBookingsWebApi.Models
{
    public class Member
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public DateTime DateJoined { get; set; }
    }
}

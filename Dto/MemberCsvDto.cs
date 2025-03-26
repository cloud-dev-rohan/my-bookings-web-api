namespace MyBookingsWebApi.Models
{
    public class MemberCsvDto
    {
        public string Name { get; set; } = string.Empty;
        public string SirName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public DateTime DateJoined { get; set; }
    }
}

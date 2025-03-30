namespace MyBookingsWebApi.Models
{
    public class MemberCsvDto
    {
        public string Name { get; set; } = string.Empty;
        public string SirName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        [CsvHelper.Configuration.Attributes.Format("yyyy-MM-dd", "MM/dd/yyyy")]
        public DateTime DateJoined { get; set; }
    }
}

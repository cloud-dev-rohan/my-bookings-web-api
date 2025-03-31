using CsvHelper.Configuration;

namespace MyBookingsWebApi.Models
{
    public class MemberCsvDto
    {
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        [CsvHelper.Configuration.Attributes.Format("yyyy-MM-dd", "MM/dd/yyyy")]
        public DateTime DateJoined { get; set; }
    }

    public class MemberCsvMap : ClassMap<MemberCsvDto>
    {
        public MemberCsvMap()
        {
            Map(m => m.Name).Name("name");
            Map(m => m.Surname).Name("surname");
            Map(m => m.BookingCount).Name("booking_count"); 
            Map(m => m.DateJoined).Name("date_joined");
        }
    }
}

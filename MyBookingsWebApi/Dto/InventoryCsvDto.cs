using CsvHelper.Configuration;

namespace MyBookingsWebApi.Models
{
    public class InventoryCsvDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RemainingCount { get; set; }

        [CsvHelper.Configuration.Attributes.Format("yyyy-MM-dd", "MM/dd/yyyy")]
        public DateTime ExpirationDate { get; set; }
        
    }

    public class InventoryCsvMap : ClassMap<InventoryCsvDto>
    {
        public InventoryCsvMap()
        {
            Map(m => m.Title).Name("title");
            Map(m => m.Description).Name("description");
            Map(m => m.RemainingCount).Name("remaining_count");
            Map(m => m.ExpirationDate).Name("expiration_date");
        }
    }
}

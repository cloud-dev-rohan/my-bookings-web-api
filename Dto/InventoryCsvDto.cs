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
}

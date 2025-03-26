namespace MyBookingsWebApi.Models
{
    public class Inventory
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public int RemainingCount { get; set; }
    }
}

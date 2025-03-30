using MassTransit;
using MyBookingsWebApi.Data;
using MyBookingsWebApi.Models;

namespace MyBookingsWebApi.Services
{
    public class InventoryBatchConsumer : IConsumer<InventoryBatchMessage>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<InventoryBatchConsumer> _logger;

        public InventoryBatchConsumer(AppDbContext dbContext, ILogger<InventoryBatchConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<InventoryBatchMessage> context)
        {
            try
            {
                _dbContext.Inventories.AddRange(context.Message.Inventories);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully processed inventory batch with {count} items", context.Message.Inventories.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inventory batch, will retry or send to dead-letter queue");
                throw;
            }
        }
    }

    public class InventoryBatchMessage
    {
        public List<Inventory> Inventories { get; set; }
    }
}
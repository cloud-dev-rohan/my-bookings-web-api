using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyBookingsWebApi.Data;
using MyBookingsWebApi.Models;

namespace MyBookingsWebApi.Services
{
    public class MemberBatchConsumer : IConsumer<MemberBatchMessage>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<MemberBatchConsumer> _logger;

        public MemberBatchConsumer(AppDbContext dbContext, ILogger<MemberBatchConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<MemberBatchMessage> context)
        {

            try
            {
                _dbContext.Members.AddRange(context.Message.Members);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Successfully processed batch with {count} members", context.Message.Members.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch, will retry or send to dead-letter queue");
                throw; // This will trigger retry policy
            }

        }
    }

    public class MemberBatchMessage
    {
        public List<Member> Members { get; set; }
    }
}
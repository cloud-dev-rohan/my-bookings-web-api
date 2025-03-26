using CsvHelper;
using Microsoft.EntityFrameworkCore;
using MyBookingsWebApi.Data;
using MyBookingsWebApi.Models;
using System.Globalization;

namespace MyBookingsWebApi.Services
{
    public class CsvUploadService : ICsvUploadService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private const int BatchSize = 1000;

        public CsvUploadService(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task UploadMembersAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = new List<Member>();

            await foreach (var record in csv.GetRecordsAsync<MemberCsvDto>())
            {
                records.Add(new Member
                {
                    Id = Guid.NewGuid(),
                    FirstName = record.Name,
                    LastName = record.SirName,
                    BookingCount = record.BookingCount,
                    DateJoined = record.DateJoined
                });
            }

            // Process in batches using LINQ chunking and parallel execution
            var batches = records.Chunk(BatchSize);
            var batchTasks = batches.Select(batch => SaveBatchAsync(batch));
            await Task.WhenAll(batchTasks);
        }

        public async Task UploadInventoryAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = new List<Inventory>();
            await foreach (var record in csv.GetRecordsAsync<InventoryCsvDto>())
            {
                records.Add(new Inventory { Id = Guid.NewGuid(), Title = record.Title, Description = record.Description, RemainingCount = record.RemainingCount });

            }
            var batches = records.Chunk(BatchSize);
            var batchTasks = batches.Select(batch => SaveBatchAsync(batch));
            await Task.WhenAll(batchTasks);


        }
        private async Task SaveBatchAsync<T>(IEnumerable<T> batch) where T : class
        {
            await using var context = _dbContextFactory.CreateDbContext();
            await context.Set<T>().AddRangeAsync(batch);
            await context.SaveChangesAsync();
        }
    }
}

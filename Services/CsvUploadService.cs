using CsvHelper;
using MyBookingsWebApi.Data;
using MyBookingsWebApi.Models;
using System.Globalization;

namespace MyBookingsWebApi.Services
{
    public class CsvUploadService : ICsvUploadService
    {
        private readonly AppDbContext _dbContext;
        private const int BatchSize = 1000; 

        public CsvUploadService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task UploadMembersAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = new List<Member>();
            await foreach (var record in csv.GetRecordsAsync<MemberCsvDto>())
            {
                records.Add(new Member { Id = Guid.NewGuid(), FirstName = record.Name, LastName = record.SirName, BookingCount = record.BookingCount, DateJoined = record.DateJoined });

                if (records.Count >= BatchSize)
                {
                    await _dbContext.Members.AddRangeAsync(records);
                    await _dbContext.SaveChangesAsync();
                    records.Clear();
                }
            }

            if (records.Count > 0) 
            {
                await _dbContext.Members.AddRangeAsync(records);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UploadInventoryAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = new List<Inventory>();
            await foreach (var record in csv.GetRecordsAsync<InventoryCsvDto>())
            {
                records.Add(new Inventory {  Id = Guid.NewGuid(), Title = record.Title, Description = record.Description, RemainingCount = record.RemainingCount });

                if (records.Count >= BatchSize)
                {
                    await _dbContext.Inventories.AddRangeAsync(records);
                    await _dbContext.SaveChangesAsync();
                    records.Clear();
                }
            }

            if (records.Count > 0)
            {
                await _dbContext.Inventories.AddRangeAsync(records);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}


using CsvHelper.Configuration;
using CsvHelper;
using MassTransit;
using MyBookingsWebApi.Models;
using System.Globalization;
using Newtonsoft.Json;

namespace MyBookingsWebApi.Services
{
    public class AsyncUploadCsvService : ICsvUploadService
    {
        private readonly IBus _bus;
        private const int BatchSize = 100;

        public AsyncUploadCsvService(IBus bus)
        {
            _bus = bus;
        }
        public async Task UploadInventoryAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower() // Case-insensitive matching
            };

            using var csv = new CsvReader(reader, config);



            var records = new List<Inventory>();
            await foreach (var record in csv.GetRecordsAsync<InventoryCsvDto>())
            {
                records.Add(new Inventory { Id = Guid.NewGuid(), Title = record.Title, Description = record.Description, RemainingCount = record.RemainingCount });

            }
            Console.WriteLine(JsonConvert.SerializeObject(records));

            var batches = records.Chunk(BatchSize);
            var endpoint = await _bus.GetSendEndpoint(new Uri("queue:inventory_batch_queue"));

            foreach (var batch in batches)
            {
                await endpoint.Send(new InventoryBatchMessage { Inventories = [.. batch] });
            }
        }

        public async Task UploadMembersAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower()
            };

            using var csv = new CsvReader(reader, config);
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

            var batches = records.Chunk(BatchSize);
            var endpoint = await _bus.GetSendEndpoint(new Uri("queue:member_batch_queue"));

            foreach (var batch in batches)
            {
                await endpoint.Send(new MemberBatchMessage { Members = [.. batch] });
            }

        }
    }
}

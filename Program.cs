using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyBookingsWebApi.Data;
using MyBookingsWebApi.Repository;
using MyBookingsWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MemberBatchConsumer>();
    x.AddConsumer<InventoryBatchConsumer>();


    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq://rabbitmq";
        var rabbitMqUser = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
        var rabbitMqPass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";

        cfg.Host(rabbitMqHost, h =>
        {
            h.Username(rabbitMqUser);
            h.Password(rabbitMqPass);
        });

        // Member Batch Queue
        cfg.ReceiveEndpoint("member_batch_queue", e =>
        {
            e.ConfigureConsumer<MemberBatchConsumer>(context);

            // Retry mechanism
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

            // Delayed redelivery
            e.UseDelayedRedelivery(r => r.Incremental(3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2)));

            // Configure DLX (Dead Letter Exchange)
        });

        // Inventory Batch Queue
        cfg.ReceiveEndpoint("inventory_batch_queue", e =>
        {
            e.ConfigureConsumer<InventoryBatchConsumer>(context);

            // Retry mechanism
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));

            // Delayed redelivery
            e.UseDelayedRedelivery(r => r.Incremental(3, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2)));

            // Configure DLX (Dead Letter Exchange)
        });

        /* // Dead-letter queue for Member
         cfg.ReceiveEndpoint("member_batch_dlq", e =>
         {
             e.ConfigureConsumer<DeadLetterConsumer>(context);
         });

         // Dead-letter queue for Inventory
         cfg.ReceiveEndpoint("inventory_batch_dlq", e =>
         {
             e.ConfigureConsumer<DeadLetterConsumer>(context);
         });*/
    });
});


// Use AsyncUploadCsvService for processing inventory upload asyn manner in batches otherwise use CsvUploadService

builder.Services.AddScoped<ICsvUploadService, AsyncUploadCsvService>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Register services
builder.Services.AddScoped<IBookingService, BookingService>();
/*builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseInMemoryDatabase("InMemoryDb"));
*/

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Automatically apply migrations at startup only for lower environment
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

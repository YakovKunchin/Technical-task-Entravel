using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using OrderProcessingService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoOptions>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<IOrdersRepository, OrdersRepository>();

builder.Services.AddSingleton<IOrderQueue, RedisOrderQueue>();
builder.Services.AddHostedService<OrderWorker>();

builder.Services.AddSingleton<MetricsService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Submit order (returns immediately)
app.MapPost("/orders", async (
    [FromBody] CreateOrderRequest input,
    IOrdersRepository repo,
    IOrderQueue queue,
    ILoggerFactory lf,
    CancellationToken ct) =>
{
    
    // for usability, it generates a random customer ID if none is provided
    if (ObjectId.TryParse(input.CustomerId, out var customerId)) { }
    else customerId = ObjectId.GenerateNewId();

    var order = new OrderDocument
    {
        CustomerId = customerId,
        Items = input.Items,
        Status = OrderStatus.Pending
    };

    var id = await repo.CreateAsync(order, ct);
    await queue.EnqueueAsync(id, ct);

    lf.CreateLogger("Orders").LogInformation("Order {OrderId} accepted for async processing.", id);

    return Results.Accepted($"/orders/{id}", new { Id = id.ToString(), Status = OrderStatus.Pending });
});

app.MapGet("/metrics", async (MetricsService metrics, CancellationToken ct) =>
{
    var processed = await metrics.SnapshotAsync(ct);
    return Results.Ok(new { ProcessedOrders = processed });
});

app.Run();

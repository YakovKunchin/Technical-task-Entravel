using MongoDB.Bson;
using MongoDB.Driver;

namespace OrderProcessingService;

public class OrdersRepository(MongoContext ctx) : IOrdersRepository
{
    public async Task<ObjectId> CreateAsync(OrderDocument order, CancellationToken ct = default)
    {
        await ctx.Orders.InsertOneAsync(order, cancellationToken: ct);
        return order.Id;
    }

    public async Task MarkProcessed(ObjectId id, CancellationToken ct = default)
    {
        var upd = Builders<OrderDocument>.Update.Set(o => o.Status, OrderStatus.Processed);
        
        await ctx.Orders.UpdateOneAsync(o => o.Id == id, upd, cancellationToken: ct);
    }

    public Task<long> CountProcessed(CancellationToken ct = default)
        => ctx.Orders.CountDocumentsAsync(o => o.Status == OrderStatus.Processed, cancellationToken: ct);
}
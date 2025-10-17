using MongoDB.Bson;

namespace OrderProcessingService;

public interface IOrdersRepository
{
    Task<ObjectId> CreateAsync(OrderDocument order, CancellationToken ct = default);
    Task MarkProcessed(ObjectId id, CancellationToken ct = default);
    Task<long> CountProcessed(CancellationToken ct = default);
}
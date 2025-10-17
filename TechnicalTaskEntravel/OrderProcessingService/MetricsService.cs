namespace OrderProcessingService;

public class MetricsService(IOrdersRepository repo)
{
    public Task<long> SnapshotAsync(CancellationToken ct = default) => repo.CountProcessed(ct);
}
using MongoDB.Bson;

namespace OrderProcessingService;

public class OrderWorker(
    IOrderQueue queue,
    IOrdersRepository repo,
    ILogger<OrderWorker> logger
) : BackgroundService
{
    private readonly int _simulatedMs = 10_000;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("OrderWorker started.");
        while (!ct.IsCancellationRequested)
        {
            ObjectId? id = null;
            try
            {
                id = await queue.DequeueAsync(ct);
                if (id is null) 
                    continue;

                // Simulate business logic
                await Task.Delay(_simulatedMs, ct);

                await repo.MarkProcessed(id.Value, ct);
                
                logger.LogInformation("Order {OrderId} processed.", id);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed processing order {OrderId}", id ?? ObjectId.Empty);
            }
            
            await Task.Delay(200, ct);
        }
        logger.LogInformation("OrderWorker stopping.");
    }
}
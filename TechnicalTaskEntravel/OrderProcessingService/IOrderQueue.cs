using MongoDB.Bson;
using StackExchange.Redis;

namespace OrderProcessingService;

public interface IOrderQueue
{
    Task EnqueueAsync(ObjectId orderId, CancellationToken ct = default);
    Task<ObjectId?> DequeueAsync(CancellationToken ct);
}

public class RedisOrderQueue : IOrderQueue, IAsyncDisposable
{
    private readonly ILogger<RedisOrderQueue> _logger;
    private readonly string _key;
    private readonly IConnectionMultiplexer _muxer;

    public RedisOrderQueue(IConfiguration cfg, ILogger<RedisOrderQueue> logger)
    {
        _logger = logger;
        _key = cfg.GetValue<string>("Redis:QueueKey") ?? "orders:queue";
        var conf = cfg.GetValue<string>("Redis:Configuration") ?? "localhost:6379";
        _muxer = ConnectionMultiplexer.Connect(conf);
    }

    public async Task EnqueueAsync(ObjectId orderId, CancellationToken ct = default)
    {
        var db = _muxer.GetDatabase();
        await db.ListLeftPushAsync(_key, orderId.ToString());
    }

    public async Task<ObjectId?> DequeueAsync(CancellationToken ct)
    {
        var db = _muxer.GetDatabase();
        
        while (!ct.IsCancellationRequested)
        {
            var res = await db.ListRightPopAsync(_key);
            if (!res.IsNullOrEmpty)
            {
                if (ObjectId.TryParse(res, out var id))
                    return id;
                
                _logger.LogError("Failed parsing order ID from Redis: {OrderId}", res);
                return null;
            }

            await Task.Delay(200, ct);
        }
        return null;
    }

    public async ValueTask DisposeAsync() => await _muxer.DisposeAsync();
}
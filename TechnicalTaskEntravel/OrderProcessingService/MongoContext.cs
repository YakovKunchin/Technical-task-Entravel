using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace OrderProcessingService;

public class MongoOptions
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "ordersdb";
    public string OrdersCollection { get; set; } = "orders";
}

public class MongoContext
{
    public IMongoDatabase Database { get; }
    public IMongoCollection<OrderDocument> Orders { get; }

    public MongoContext(IOptions<MongoOptions> cfg)
    {
        var client = new MongoClient(cfg.Value.ConnectionString);
        Database = client.GetDatabase(cfg.Value.Database);
        Orders = Database.GetCollection<OrderDocument>(cfg.Value.OrdersCollection);
        
        var indexModels = new List<CreateIndexModel<OrderDocument>>
        {
            new(Builders<OrderDocument>.IndexKeys.Ascending(x => x.CustomerId)),
            new(Builders<OrderDocument>.IndexKeys.Ascending(x => x.Status)),
            new(Builders<OrderDocument>.IndexKeys.Ascending(x => x.TotalAmount)),
        };

        Orders.Indexes.CreateMany(indexModels);
    }
}
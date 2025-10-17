using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrderProcessingService;

using MongoDB.Bson;

public enum OrderStatus
{
    None,
    Pending,
    Processed,
    Failed
}
public class OrderDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

    public required ObjectId CustomerId { get; set; }
    public required List<string> Items { get; set; }

    public decimal TotalAmount
    {
        get => Items.Count;
        private set => _ = value;
    }
    
    public required OrderStatus Status { get; set; }
}

public class CreateOrderRequest
{
    public string CustomerId { get; set; } = string.Empty;
    public List<string> Items { get; set; } = [];
}
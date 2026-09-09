namespace OrderTracker.Api.Models;

public sealed class Order
{
    public Guid Id { get; set; }
    public required string OrderNumber { get; set; }
    public string? ClientReference { get; set; }
    public List<LineItem> LineItems { get; set; } = [];
    public string? Notes { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

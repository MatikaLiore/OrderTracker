namespace OrderTracker.Api.Domain;

public sealed class Order
{
    public Guid Id { get; init; }
    public required string ExternalReference { get; init; }
    public required Customer Customer { get; init; }
    public required IReadOnlyList<LineItem> LineItems { get; init; }
    public required string Currency { get; init; }
    public string? Notes { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Total { get; init; }
    public OrderStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}

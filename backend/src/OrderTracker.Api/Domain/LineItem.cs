namespace OrderTracker.Api.Domain;

public sealed class LineItem
{
    public required string Sku { get; init; }
    public required string Name { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

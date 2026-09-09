namespace OrderTracker.Api.Models;

public sealed class MenuItem
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public decimal UnitPrice { get; set; }
}

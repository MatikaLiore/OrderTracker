namespace OrderTracker.Api.Contracts;

public sealed class CreateOrderRequest
{
    public string ExternalReference { get; set; } = string.Empty;
    public CustomerInput? Customer { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<LineItemInput> LineItems { get; set; } = [];
}

public sealed class CustomerInput
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class LineItemInput
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

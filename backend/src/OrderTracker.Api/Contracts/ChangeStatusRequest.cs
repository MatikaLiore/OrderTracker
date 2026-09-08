using OrderTracker.Api.Domain;

namespace OrderTracker.Api.Contracts;

public sealed class ChangeStatusRequest
{
    public OrderStatus Status { get; set; }
}

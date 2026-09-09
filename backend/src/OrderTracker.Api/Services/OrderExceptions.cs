using OrderTracker.Api.Models;

namespace OrderTracker.Api.Services;

public sealed class OrderValidationException : Exception
{
    public OrderValidationException(IReadOnlyList<string> errors)
        : base(errors.Count == 1 ? errors[0] : "The food order could not be accepted.")
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}

public sealed class BadStatusChangeException : Exception
{
    public BadStatusChangeException(OrderStatus from, OrderStatus to)
        : base(OrderStatusRules.Explain(from, to))
    {
        From = from;
        To = to;
    }

    public OrderStatus From { get; }
    public OrderStatus To { get; }
}

public sealed class OrderNotFoundException : Exception
{
    public OrderNotFoundException(Guid id)
        : base($"No food order found with id {id}.")
    {
        OrderId = id;
    }

    public Guid OrderId { get; }
}

using OrderTracker.Api.Domain;

namespace OrderTracker.Api.Services;

public sealed class OrderValidationException : Exception
{
    public OrderValidationException(IReadOnlyList<string> errors)
        : base(errors.Count == 1 ? errors[0] : "The order could not be accepted.")
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}

public sealed class DuplicateOrderException : Exception
{
    public DuplicateOrderException(string externalReference)
        : base(
            $"An order with reference '{externalReference}' already exists, but the submitted details are different. " +
            "If this is the same purchase, send the original details again. Otherwise use a new reference.")
    {
        ExternalReference = externalReference;
    }

    public string ExternalReference { get; }
}

public sealed class InvalidStatusTransitionException : Exception
{
    public InvalidStatusTransitionException(OrderStatus from, OrderStatus to)
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
        : base($"No order found with id {id}.")
    {
        OrderId = id;
    }

    public Guid OrderId { get; }
}

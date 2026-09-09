namespace OrderTracker.Api.Models;

/// <summary>Which status changes are allowed in the kitchen flow.</summary>
public static class OrderStatusRules
{
    public static bool CanChange(OrderStatus from, OrderStatus to)
    {
        if (from == to)
            return false;

        return (from, to) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Ready) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Ready, OrderStatus.Done) => true,
            _ => false
        };
    }

    public static string Explain(OrderStatus from, OrderStatus to)
    {
        if (from == to)
            return $"The order is already {from}.";

        if (from == OrderStatus.Done)
            return "Done orders are closed — status can't be changed after that.";

        if (from == OrderStatus.Cancelled)
            return "Cancelled orders are closed. Place a new food order if you need to continue.";

        if (from == OrderStatus.Pending && to is OrderStatus.Ready or OrderStatus.Done)
            return "Confirm the order before moving it further.";

        if (from == OrderStatus.Confirmed && to == OrderStatus.Done)
            return "Mark the order ready before marking it done.";

        if (from == OrderStatus.Ready && to == OrderStatus.Cancelled)
            return "Ready orders can't be cancelled — mark them done instead.";

        return $"Can't move an order from {from} to {to}.";
    }
}

namespace OrderTracker.Api.Domain;

public static class OrderStatusRules
{
    public static bool CanTransition(OrderStatus from, OrderStatus to)
    {
        if (from == to)
            return false;

        return (from, to) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Fulfilled) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            _ => false
        };
    }

    public static string Explain(OrderStatus from, OrderStatus to)
    {
        if (from == to)
            return $"The order is already {from}.";

        if (from == OrderStatus.Fulfilled)
            return "Fulfilled orders are done — status can't be changed after that.";

        if (from == OrderStatus.Cancelled)
            return "Cancelled orders are closed. Start a new order if you need to continue.";

        if (from == OrderStatus.Pending && to == OrderStatus.Fulfilled)
            return "Confirm the order before marking it fulfilled.";

        return $"Can't move an order from {from} to {to}.";
    }
}

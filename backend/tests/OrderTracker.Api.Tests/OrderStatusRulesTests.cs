using OrderTracker.Api.Domain;

namespace OrderTracker.Api.Tests;

public class OrderStatusRulesTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Fulfilled, false)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Fulfilled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Fulfilled, OrderStatus.Cancelled, false)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Pending, OrderStatus.Pending, false)]
    public void Allowed_moves_match_the_sales_flow(OrderStatus from, OrderStatus to, bool allowed)
    {
        Assert.Equal(allowed, OrderStatusRules.CanTransition(from, to));
    }

    [Fact]
    public void Explain_mentions_why_fulfilled_is_locked()
    {
        var message = OrderStatusRules.Explain(OrderStatus.Fulfilled, OrderStatus.Cancelled);
        Assert.Contains("done", message, StringComparison.OrdinalIgnoreCase);
    }
}

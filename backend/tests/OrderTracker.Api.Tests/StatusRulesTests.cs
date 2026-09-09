using OrderTracker.Api.Models;

namespace OrderTracker.Api.Tests;

public class StatusRulesTests
{
    [Fact]
    public void Pending_can_be_confirmed()
    {
        Assert.True(OrderStatusRules.CanChange(OrderStatus.Pending, OrderStatus.Confirmed));
    }

    [Fact]
    public void Pending_cannot_jump_to_done()
    {
        Assert.False(OrderStatusRules.CanChange(OrderStatus.Pending, OrderStatus.Done));
    }

    [Fact]
    public void Ready_can_be_marked_done()
    {
        Assert.True(OrderStatusRules.CanChange(OrderStatus.Ready, OrderStatus.Done));
    }

    [Fact]
    public void Done_orders_stay_closed()
    {
        Assert.False(OrderStatusRules.CanChange(OrderStatus.Done, OrderStatus.Cancelled));
        Assert.Contains("closed", OrderStatusRules.Explain(OrderStatus.Done, OrderStatus.Cancelled), StringComparison.OrdinalIgnoreCase);
    }
}

using Microsoft.EntityFrameworkCore;
using OrderTracker.Api.Data;
using OrderTracker.Api.Dtos;
using OrderTracker.Api.Models;
using OrderTracker.Api.Services;

namespace OrderTracker.Api.Tests;

/// <summary>Simple checks for the main order rules.</summary>
public class OrderTests
{
    private static OrderService CreateService()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        MenuData.Seed(db);

        return new OrderService(new OrderStore(db));
    }

    private static CreateOrderRequest SampleOrder(string? clientReference = null) => new()
    {
        ClientReference = clientReference,
        Notes = "No onions",
        LineItems =
        [
            new OrderLineRequest { MenuItemId = MenuData.BurgerId, Quantity = 2 },
            new OrderLineRequest { MenuItemId = MenuData.ChipsId, Quantity = 1 }
        ]
    };

    [Fact]
    public void New_order_gets_number_price_and_pending_status()
    {
        var result = CreateService().PlaceOrder(SampleOrder());

        Assert.True(result.Created);
        Assert.Equal("ORD-0001", result.Order!.OrderNumber);
        Assert.Equal(OrderStatus.Pending, result.Order.Status);
        Assert.Equal(175.00m, result.Order.Total); // 2×75 + 1×25
    }

    [Fact]
    public void Can_place_more_than_one_order()
    {
        var service = CreateService();

        var first = service.PlaceOrder(SampleOrder());
        var second = service.PlaceOrder(SampleOrder());

        Assert.Equal("ORD-0001", first.Order!.OrderNumber);
        Assert.Equal("ORD-0002", second.Order!.OrderNumber);
        Assert.Equal(2, service.ListOrders().Count);
    }

    [Fact]
    public void Same_client_reference_does_not_create_a_duplicate()
    {
        var service = CreateService();

        var first = service.PlaceOrder(SampleOrder("Year End Function"));
        var second = service.PlaceOrder(SampleOrder("Year End Function"));

        Assert.False(second.Created);
        Assert.Equal(first.Order!.Id, second.Order!.Id);
        Assert.Single(service.ListOrders());
    }

    [Fact]
    public void Same_reference_with_different_food_is_a_conflict()
    {
        var service = CreateService();
        service.PlaceOrder(SampleOrder("Year End Function"));

        var changed = SampleOrder("Year End Function");
        changed.LineItems[0].Quantity = 99;

        var result = service.PlaceOrder(changed);

        Assert.True(result.HasConflict);
        Assert.Single(service.ListOrders());
    }

    [Fact]
    public void Quantity_must_be_at_least_one()
    {
        var request = SampleOrder();
        request.LineItems[0].Quantity = 0;

        Assert.Throws<OrderValidationException>(() => CreateService().PlaceOrder(request));
    }

    [Fact]
    public void Status_can_move_pending_confirmed_ready_done()
    {
        var service = CreateService();
        var id = service.PlaceOrder(SampleOrder()).Order!.Id;

        service.ChangeStatus(id, OrderStatus.Confirmed);
        service.ChangeStatus(id, OrderStatus.Ready);

        Assert.Equal(OrderStatus.Done, service.ChangeStatus(id, OrderStatus.Done).Status);
    }

    [Fact]
    public void Cannot_jump_straight_to_done()
    {
        var service = CreateService();
        var id = service.PlaceOrder(SampleOrder()).Order!.Id;

        Assert.Throws<BadStatusChangeException>(() => service.ChangeStatus(id, OrderStatus.Done));
    }

    [Fact]
    public void Unknown_order_is_not_found()
    {
        Assert.Throws<OrderNotFoundException>(() => CreateService().GetOrder(Guid.NewGuid()));
    }
}

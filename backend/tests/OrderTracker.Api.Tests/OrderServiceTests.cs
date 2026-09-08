using OrderTracker.Api.Contracts;
using OrderTracker.Api.Domain;
using OrderTracker.Api.Services;

namespace OrderTracker.Api.Tests;

public class OrderServiceTests
{
    private static OrderService NewService() => new(new InMemoryOrderStore());

    [Fact]
    public void Submit_sets_pending_and_computes_totals_on_the_server()
    {
        var service = NewService();

        var result = service.Submit(ValidRequest());

        Assert.True(result.Created);
        Assert.Equal(OrderStatus.Pending, result.Order.Status);
        Assert.Equal("USD", result.Order.Currency);
        Assert.Equal(2, result.Order.LineItems.Count);
        Assert.Equal(20.50m, result.Order.LineItems[0].LineTotal);
        Assert.Equal(4.50m, result.Order.LineItems[1].LineTotal);
        Assert.Equal(25.00m, result.Order.Subtotal);
        Assert.Equal(result.Order.Subtotal, result.Order.Total);
    }

    [Fact]
    public void Submit_rounds_money_to_two_decimals()
    {
        var request = ValidRequest();
        request.LineItems =
        [
            new LineItemInput { Sku = "A-1", Name = "Odd price", Quantity = 2, UnitPrice = 10.125m }
        ];

        var order = NewService().Submit(request).Order;

        Assert.Equal(10.13m, order.LineItems[0].UnitPrice);
        Assert.Equal(20.26m, order.LineItems[0].LineTotal);
        Assert.Equal(20.26m, order.Total);
    }

    [Fact]
    public void Submit_same_reference_and_details_returns_the_original_order()
    {
        var service = NewService();
        var first = service.Submit(ValidRequest("PO-55"));
        var second = service.Submit(ValidRequest("PO-55"));

        Assert.False(second.Created);
        Assert.Equal(first.Order.Id, second.Order.Id);
        Assert.Single(service.List());
    }

    [Fact]
    public void Submit_treats_reference_as_case_insensitive_and_trims_it()
    {
        var service = NewService();
        service.Submit(ValidRequest("  po-55  "));
        var again = service.Submit(ValidRequest("PO-55"));

        Assert.False(again.Created);
        Assert.Single(service.List());
    }

    [Fact]
    public void Submit_same_reference_but_different_lines_is_rejected()
    {
        var service = NewService();
        service.Submit(ValidRequest("PO-55"));

        var changed = ValidRequest("PO-55");
        changed.LineItems[0].Quantity = 99;

        var ex = Assert.Throws<DuplicateOrderException>(() => service.Submit(changed));
        Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(service.List());
    }

    [Fact]
    public void Submit_rejects_bad_quantities_and_prices()
    {
        var service = NewService();
        var request = ValidRequest();
        request.LineItems[0].Quantity = 0;
        request.LineItems[1].UnitPrice = -1;

        var ex = Assert.Throws<OrderValidationException>(() => service.Submit(request));
        Assert.Contains(ex.Errors, e => e.Contains("quantity", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(ex.Errors, e => e.Contains("negative", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Submit_requires_at_least_one_line()
    {
        var request = ValidRequest();
        request.LineItems.Clear();

        Assert.Throws<OrderValidationException>(() => NewService().Submit(request));
    }

    [Fact]
    public void List_returns_newest_first()
    {
        var clock = new StepClock(new DateTimeOffset(2026, 3, 2, 9, 0, 0, TimeSpan.Zero));
        var service = new OrderService(new InMemoryOrderStore(), clock);

        var older = service.Submit(ValidRequest("PO-1")).Order;
        clock.Advance(TimeSpan.FromMinutes(1));
        var newer = service.Submit(ValidRequest("PO-2")).Order;

        var list = service.List();

        Assert.Equal(newer.Id, list[0].Id);
        Assert.Equal(older.Id, list[1].Id);
    }

    [Fact]
    public void ChangeStatus_pending_to_confirmed_to_fulfilled()
    {
        var service = NewService();
        var id = service.Submit(ValidRequest()).Order.Id;

        var confirmed = service.ChangeStatus(id, OrderStatus.Confirmed);
        Assert.Equal(OrderStatus.Confirmed, confirmed.Status);

        var fulfilled = service.ChangeStatus(id, OrderStatus.Fulfilled);
        Assert.Equal(OrderStatus.Fulfilled, fulfilled.Status);
        Assert.True(fulfilled.UpdatedAt >= fulfilled.CreatedAt);
    }

    [Fact]
    public void ChangeStatus_rejected_transitions_include_a_usable_message()
    {
        var service = NewService();
        var id = service.Submit(ValidRequest()).Order.Id;

        var ex = Assert.Throws<InvalidStatusTransitionException>(
            () => service.ChangeStatus(id, OrderStatus.Fulfilled));

        Assert.Contains("Confirm", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ChangeStatus_cannot_reopen_a_cancelled_order()
    {
        var service = NewService();
        var id = service.Submit(ValidRequest()).Order.Id;
        service.ChangeStatus(id, OrderStatus.Cancelled);

        Assert.Throws<InvalidStatusTransitionException>(
            () => service.ChangeStatus(id, OrderStatus.Pending));
    }

    [Fact]
    public void Get_unknown_id_throws()
    {
        Assert.Throws<OrderNotFoundException>(() => NewService().Get(Guid.NewGuid()));
    }

    private sealed class StepClock : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public StepClock(DateTimeOffset utcNow) => _utcNow = utcNow;

        public void Advance(TimeSpan by) => _utcNow += by;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    private static CreateOrderRequest ValidRequest(string reference = "PO-1001") => new()
    {
        ExternalReference = reference,
        Customer = new CustomerInput { Name = "Northwind Traders", Email = "buyer@northwind.test" },
        Currency = "usd",
        Notes = "Dock 4",
        LineItems =
        [
            new LineItemInput { Sku = "WDG-1", Name = "Widget", Quantity = 2, UnitPrice = 10.25m },
            new LineItemInput { Sku = "BLT-9", Name = "Bolt pack", Quantity = 1, UnitPrice = 4.50m }
        ]
    };
}

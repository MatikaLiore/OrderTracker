using OrderTracker.Api.Data;
using OrderTracker.Api.Dtos;
using OrderTracker.Api.Models;

namespace OrderTracker.Api.Services;

/// <summary>
/// Business rules for food orders: validate, place, list, change status.
/// Prices and totals are always calculated here on the server.
/// </summary>
public sealed class OrderService
{
    private readonly OrderStore _store;
    private readonly TimeProvider _clock;

    public OrderService(OrderStore store, TimeProvider? clock = null)
    {
        _store = store;
        _clock = clock ?? TimeProvider.System;
    }

    public SubmitResult PlaceOrder(CreateOrderRequest request)
    {
        var errors = Validate(request);
        var menuIds = request.LineItems
            .Where(l => l.MenuItemId != Guid.Empty)
            .Select(l => l.MenuItemId)
            .Distinct()
            .ToList();
        var menuById = _store.GetMenuItems(menuIds);

        foreach (var id in menuIds)
        {
            if (!menuById.ContainsKey(id))
                errors.Add("One or more menu items are not on the menu.");
        }

        if (errors.Count > 0)
            throw new OrderValidationException(errors);

        var clientRef = string.IsNullOrWhiteSpace(request.ClientReference)
            ? null
            : request.ClientReference.Trim();

        if (clientRef is not null)
        {
            var existing = _store.FindByClientReference(clientRef);
            if (existing is not null)
            {
                if (SameDetails(existing, request, menuById))
                    return new SubmitResult { Order = existing, Created = false };

                return new SubmitResult
                {
                    ConflictMessage =
                        $"A food order with client reference '{clientRef}' already exists, but the submitted details are different. " +
                        "If this is the same order, send the original details again. Otherwise use a new client reference (or leave it blank)."
                };
            }
        }

        var order = BuildOrder(request, _store.NextOrderNumber(), menuById, _clock.GetUtcNow());
        _store.Add(order);
        return new SubmitResult { Order = order, Created = true };
    }

    public IReadOnlyList<Order> ListOrders() => _store.GetAllNewestFirst();

    public Order GetOrder(Guid id) =>
        _store.FindById(id) ?? throw new OrderNotFoundException(id);

    public Order ChangeStatus(Guid id, OrderStatus next)
    {
        var order = GetOrder(id);

        if (!OrderStatusRules.CanChange(order.Status, next))
            throw new BadStatusChangeException(order.Status, next);

        order.Status = next;
        order.UpdatedAt = _clock.GetUtcNow();
        _store.Save();
        return order;
    }

    public IReadOnlyList<MenuItemDto> ListMenu() =>
        _store.GetMenu()
            .Select(m => new MenuItemDto
            {
                Id = m.Id,
                Code = m.Code,
                Name = m.Name,
                UnitPrice = m.UnitPrice
            })
            .ToList();

    private static List<string> Validate(CreateOrderRequest request)
    {
        // Shape checks (length, quantity, required lines) also live as DataAnnotations on the DTOs.
        // Keep these here so OrderService stays safe when called from unit tests without the HTTP pipeline.
        var errors = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.ClientReference) && request.ClientReference.Trim().Length > 80)
            errors.Add("Client reference is too long (80 characters max).");

        if (!string.IsNullOrWhiteSpace(request.Notes) && request.Notes.Trim().Length > 1000)
            errors.Add("Notes are too long (1000 characters max).");

        if (request.LineItems is null || request.LineItems.Count == 0)
        {
            errors.Add("At least one menu item is required.");
            return errors;
        }

        if (request.LineItems.Count > 50)
            errors.Add("An order can have at most 50 menu items.");

        for (var i = 0; i < request.LineItems.Count; i++)
        {
            var line = request.LineItems[i];
            var n = i + 1;

            if (line.MenuItemId == Guid.Empty)
                errors.Add($"Item {n}: please select a dish from the menu.");
            if (line.Quantity < 1)
                errors.Add($"Item {n}: quantity must be a positive whole number.");
        }

        return errors;
    }

    private static Order BuildOrder(
        CreateOrderRequest request,
        string orderNumber,
        IReadOnlyDictionary<Guid, MenuItem> menuById,
        DateTimeOffset now)
    {
        var lines = request.LineItems.Select(input =>
        {
            var menu = menuById[input.MenuItemId];
            var unitPrice = RoundMoney(menu.UnitPrice);
            return new LineItem
            {
                Id = Guid.NewGuid(),
                MenuItemId = menu.Id,
                MenuCode = menu.Code,
                Name = menu.Name,
                Quantity = input.Quantity,
                UnitPrice = unitPrice,
                LineTotal = RoundMoney(input.Quantity * unitPrice)
            };
        }).ToList();

        var subtotal = lines.Sum(l => l.LineTotal);

        return new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            ClientReference = string.IsNullOrWhiteSpace(request.ClientReference)
                ? null
                : request.ClientReference.Trim(),
            LineItems = lines,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Subtotal = subtotal,
            Total = subtotal,
            Status = OrderStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static bool SameDetails(
        Order existing,
        CreateOrderRequest request,
        IReadOnlyDictionary<Guid, MenuItem> menuById)
    {
        var existingNotes = existing.Notes ?? string.Empty;
        var incomingNotes = string.IsNullOrWhiteSpace(request.Notes) ? string.Empty : request.Notes.Trim();
        if (!string.Equals(existingNotes, incomingNotes, StringComparison.Ordinal))
            return false;

        if (existing.LineItems.Count != request.LineItems.Count)
            return false;

        var existingLines = existing.LineItems
            .Select(l => (l.MenuItemId, l.Quantity, l.UnitPrice))
            .OrderBy(l => l.MenuItemId)
            .ToList();

        var incomingLines = request.LineItems
            .Select(l => (MenuItemId: l.MenuItemId, l.Quantity, UnitPrice: RoundMoney(menuById[l.MenuItemId].UnitPrice)))
            .OrderBy(l => l.MenuItemId)
            .ToList();

        for (var i = 0; i < existingLines.Count; i++)
        {
            if (existingLines[i].MenuItemId != incomingLines[i].MenuItemId)
                return false;
            if (existingLines[i].Quantity != incomingLines[i].Quantity)
                return false;
            if (existingLines[i].UnitPrice != incomingLines[i].UnitPrice)
                return false;
        }

        return true;
    }

    private static decimal RoundMoney(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}

using System.Net.Mail;
using System.Text.RegularExpressions;
using OrderTracker.Api.Contracts;
using OrderTracker.Api.Domain;

namespace OrderTracker.Api.Services;

public sealed class OrderService
{
    private static readonly Regex CurrencyCode = new("^[A-Za-z]{3}$", RegexOptions.Compiled);

    private readonly IOrderStore _store;
    private readonly TimeProvider _time;

    public OrderService(IOrderStore store) : this(store, TimeProvider.System)
    {
    }

    public OrderService(IOrderStore store, TimeProvider time)
    {
        _store = store;
        _time = time;
    }

    public SubmitOrderResult Submit(CreateOrderRequest request)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
            throw new OrderValidationException(errors);

        var reference = request.ExternalReference.Trim();

        if (_store.TryGetByReference(reference, out var existing) && existing is not null)
            return ReplayOrReject(existing, request);

        var order = Build(request, reference, _time.GetUtcNow());
        if (!_store.TryAdd(order))
        {
            // Lost a race with another submit of the same reference.
            if (!_store.TryGetByReference(reference, out var winner) || winner is null)
                throw new InvalidOperationException("Order was not stored.");

            return ReplayOrReject(winner, request);
        }

        return new SubmitOrderResult(order, Created: true);
    }

    public IReadOnlyList<Order> List() => _store.ListNewestFirst();

    public Order Get(Guid id)
    {
        if (!_store.TryGetById(id, out var order) || order is null)
            throw new OrderNotFoundException(id);

        return order;
    }

    public Order ChangeStatus(Guid id, OrderStatus next)
    {
        var order = Get(id);

        lock (order)
        {
            if (!OrderStatusRules.CanTransition(order.Status, next))
                throw new InvalidStatusTransitionException(order.Status, next);

            order.Status = next;
            order.UpdatedAt = _time.GetUtcNow();
            return order;
        }
    }

    private static SubmitOrderResult ReplayOrReject(Order existing, CreateOrderRequest request)
    {
        if (SamePayload(existing, request))
            return new SubmitOrderResult(existing, Created: false);

        throw new DuplicateOrderException(existing.ExternalReference);
    }

    private static Order Build(CreateOrderRequest request, string reference, DateTimeOffset now)
    {
        var items = request.LineItems.Select(ToLineItem).ToList();
        var subtotal = items.Sum(i => i.LineTotal);

        return new Order
        {
            Id = Guid.NewGuid(),
            ExternalReference = reference,
            Customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = request.Customer!.Name.Trim(),
                Email = request.Customer.Email.Trim()
            },
            LineItems = items,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            Subtotal = subtotal,
            Total = subtotal,
            Status = OrderStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static LineItem ToLineItem(LineItemInput input)
    {
        var unitPrice = Money.Round(input.UnitPrice);
        return new LineItem
        {
            Sku = input.Sku.Trim(),
            Name = input.Name.Trim(),
            Quantity = input.Quantity,
            UnitPrice = unitPrice,
            LineTotal = Money.Round(input.Quantity * unitPrice)
        };
    }

    private static List<string> Validate(CreateOrderRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.ExternalReference))
            errors.Add("External reference is required.");
        else if (request.ExternalReference.Trim().Length > 80)
            errors.Add("External reference is too long (80 characters max).");

        if (request.Customer is null)
        {
            errors.Add("Customer is required.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.Customer.Name))
                errors.Add("Customer name is required.");
            else if (request.Customer.Name.Trim().Length > 120)
                errors.Add("Customer name is too long.");

            if (string.IsNullOrWhiteSpace(request.Customer.Email))
                errors.Add("Customer email is required.");
            else if (!MailAddress.TryCreate(request.Customer.Email.Trim(), out _))
                errors.Add("Customer email doesn't look valid.");
        }

        if (string.IsNullOrWhiteSpace(request.Currency) || !CurrencyCode.IsMatch(request.Currency.Trim()))
            errors.Add("Currency must be a 3-letter code, e.g. USD.");

        if (!string.IsNullOrWhiteSpace(request.Notes) && request.Notes.Trim().Length > 1000)
            errors.Add("Notes are too long (1000 characters max).");

        if (request.LineItems is null || request.LineItems.Count == 0)
        {
            errors.Add("At least one line item is required.");
            return errors;
        }

        if (request.LineItems.Count > 50)
            errors.Add("An order can have at most 50 line items.");

        for (var i = 0; i < request.LineItems.Count; i++)
        {
            var line = request.LineItems[i];
            var n = i + 1;

            if (string.IsNullOrWhiteSpace(line.Sku))
                errors.Add($"Line {n}: SKU is required.");
            if (string.IsNullOrWhiteSpace(line.Name))
                errors.Add($"Line {n}: name is required.");
            if (line.Quantity < 1)
                errors.Add($"Line {n}: quantity must be a positive whole number.");
            if (line.UnitPrice < 0)
                errors.Add($"Line {n}: unit price can't be negative.");
        }

        return errors;
    }

    private static bool SamePayload(Order existing, CreateOrderRequest request)
    {
        if (!string.Equals(existing.Currency, request.Currency.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        var existingNotes = existing.Notes ?? string.Empty;
        var incomingNotes = string.IsNullOrWhiteSpace(request.Notes) ? string.Empty : request.Notes.Trim();
        if (!string.Equals(existingNotes, incomingNotes, StringComparison.Ordinal))
            return false;

        if (!string.Equals(existing.Customer.Name, request.Customer!.Name.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.Equals(existing.Customer.Email, request.Customer.Email.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        if (existing.LineItems.Count != request.LineItems.Count)
            return false;

        var existingLines = existing.LineItems
            .Select(l => (l.Sku, l.Name, l.Quantity, l.UnitPrice))
            .OrderBy(l => l.Sku, StringComparer.OrdinalIgnoreCase)
            .ThenBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var incomingLines = request.LineItems
            .Select(l => (Sku: l.Sku.Trim(), Name: l.Name.Trim(), l.Quantity, UnitPrice: Money.Round(l.UnitPrice)))
            .OrderBy(l => l.Sku, StringComparer.OrdinalIgnoreCase)
            .ThenBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        for (var i = 0; i < existingLines.Count; i++)
        {
            var a = existingLines[i];
            var b = incomingLines[i];
            if (!string.Equals(a.Sku, b.Sku, StringComparison.OrdinalIgnoreCase))
                return false;
            if (!string.Equals(a.Name, b.Name, StringComparison.OrdinalIgnoreCase))
                return false;
            if (a.Quantity != b.Quantity)
                return false;
            if (a.UnitPrice != b.UnitPrice)
                return false;
        }

        return true;
    }
}

internal static class Money
{
    public static decimal Round(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}

using System.Collections.Concurrent;
using OrderTracker.Api.Domain;

namespace OrderTracker.Api.Services;

public sealed class InMemoryOrderStore : IOrderStore
{
    private readonly ConcurrentDictionary<Guid, Order> _byId = new();
    private readonly ConcurrentDictionary<string, Order> _byReference =
        new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Order> ListNewestFirst() =>
        _byId.Values
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .ToList();

    public bool TryGetById(Guid id, out Order? order) =>
        _byId.TryGetValue(id, out order);

    public bool TryGetByReference(string externalReference, out Order? order) =>
        _byReference.TryGetValue(externalReference.Trim(), out order);

    public bool TryAdd(Order order)
    {
        if (!_byReference.TryAdd(order.ExternalReference, order))
            return false;

        if (!_byId.TryAdd(order.Id, order))
        {
            _byReference.TryRemove(order.ExternalReference, out _);
            return false;
        }

        return true;
    }
}

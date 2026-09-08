using OrderTracker.Api.Domain;

namespace OrderTracker.Api.Services;

public interface IOrderStore
{
    IReadOnlyList<Order> ListNewestFirst();
    bool TryGetById(Guid id, out Order? order);
    bool TryGetByReference(string externalReference, out Order? order);
    bool TryAdd(Order order);
}

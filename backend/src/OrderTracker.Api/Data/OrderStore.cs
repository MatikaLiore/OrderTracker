using Microsoft.EntityFrameworkCore;
using OrderTracker.Api.Models;

namespace OrderTracker.Api.Data;

/// <summary>Simple database access for orders and menu items.</summary>
public sealed class OrderStore
{
    private readonly AppDbContext _db;

    public OrderStore(AppDbContext db) => _db = db;

    public IReadOnlyList<Order> GetAllNewestFirst() =>
        _db.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .ToList();

    public Order? FindById(Guid id) =>
        _db.Orders.FirstOrDefault(o => o.Id == id);

    public Order? FindByClientReference(string clientReference) =>
        _db.Orders.FirstOrDefault(o =>
            o.ClientReference != null &&
            o.ClientReference.ToLower() == clientReference.Trim().ToLower());

    public string NextOrderNumber()
    {
        var count = _db.Orders.Count();
        return $"ORD-{(count + 1):D4}";
    }

    public void Add(Order order)
    {
        _db.Orders.Add(order);
        _db.SaveChanges();
    }

    public void Save() => _db.SaveChanges();

    public IReadOnlyList<MenuItem> GetMenu() =>
        _db.MenuItems.AsNoTracking().OrderBy(m => m.Name).ToList();

    public Dictionary<Guid, MenuItem> GetMenuItems(IEnumerable<Guid> ids)
    {
        var idList = ids.Distinct().ToList();
        return _db.MenuItems
            .AsNoTracking()
            .Where(m => idList.Contains(m.Id))
            .ToDictionary(m => m.Id);
    }
}

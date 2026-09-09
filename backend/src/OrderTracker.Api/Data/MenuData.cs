using OrderTracker.Api.Models;

namespace OrderTracker.Api.Data;

/// <summary>Starter menu loaded into the database on startup.</summary>
public static class MenuData
{
    public static readonly Guid PizzaId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    public static readonly Guid BurgerId = Guid.Parse("11111111-1111-1111-1111-111111111102");
    public static readonly Guid PapWorsId = Guid.Parse("11111111-1111-1111-1111-111111111103");
    public static readonly Guid PapSteakId = Guid.Parse("11111111-1111-1111-1111-111111111104");
    public static readonly Guid ChickenCurryId = Guid.Parse("11111111-1111-1111-1111-111111111105");
    public static readonly Guid BeefStewId = Guid.Parse("11111111-1111-1111-1111-111111111106");
    public static readonly Guid FishChipsId = Guid.Parse("11111111-1111-1111-1111-111111111107");
    public static readonly Guid GrilledChickenId = Guid.Parse("11111111-1111-1111-1111-111111111108");
    public static readonly Guid VeggieWrapId = Guid.Parse("11111111-1111-1111-1111-111111111109");
    public static readonly Guid ChipsId = Guid.Parse("11111111-1111-1111-1111-111111111110");
    public static readonly Guid SoftDrinkId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid MilkshakeId = Guid.Parse("11111111-1111-1111-1111-111111111112");

    public static IReadOnlyList<MenuItem> Items { get; } =
    [
        new() { Id = PizzaId, Code = "PZA-01", Name = "Pizza Margherita", UnitPrice = 89.00m },
        new() { Id = BurgerId, Code = "BRG-01", Name = "Classic Burger", UnitPrice = 75.00m },
        new() { Id = PapWorsId, Code = "PAP-01", Name = "Pap & Wors", UnitPrice = 65.00m },
        new() { Id = PapSteakId, Code = "PAP-02", Name = "Pap and Steak", UnitPrice = 95.00m },
        new() { Id = ChickenCurryId, Code = "CUR-01", Name = "Chicken Curry", UnitPrice = 80.00m },
        new() { Id = BeefStewId, Code = "STW-01", Name = "Beef Stew", UnitPrice = 85.00m },
        new() { Id = FishChipsId, Code = "FSH-01", Name = "Fish and Chips", UnitPrice = 78.00m },
        new() { Id = GrilledChickenId, Code = "CHK-01", Name = "Grilled Chicken", UnitPrice = 72.00m },
        new() { Id = VeggieWrapId, Code = "VEG-01", Name = "Veggie Wrap", UnitPrice = 55.00m },
        new() { Id = ChipsId, Code = "SID-01", Name = "Chips", UnitPrice = 25.00m },
        new() { Id = SoftDrinkId, Code = "DRK-01", Name = "Soft Drink", UnitPrice = 18.00m },
        new() { Id = MilkshakeId, Code = "DRK-02", Name = "Milkshake", UnitPrice = 35.00m }
    ];

    public static void Seed(AppDbContext db)
    {
        if (db.MenuItems.Any())
            return;

        db.MenuItems.AddRange(Items);
        db.SaveChanges();
    }
}

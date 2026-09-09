using Microsoft.EntityFrameworkCore;
using OrderTracker.Api.Models;

namespace OrderTracker.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(40).IsRequired();
            e.Property(x => x.Name).HasMaxLength(120).IsRequired();
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.OrderNumber).HasMaxLength(40).IsRequired();
            e.HasIndex(x => x.OrderNumber).IsUnique();
            e.Property(x => x.ClientReference).HasMaxLength(80);
            e.HasIndex(x => x.ClientReference).IsUnique();
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.Property(x => x.Subtotal).HasPrecision(18, 2);
            e.Property(x => x.Total).HasPrecision(18, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);

            e.OwnsMany(x => x.LineItems, li =>
            {
                li.ToTable("OrderLineItems");
                li.WithOwner().HasForeignKey("OrderId");
                li.HasKey(x => x.Id);
                li.Property(x => x.MenuCode).HasMaxLength(40).IsRequired();
                li.Property(x => x.Name).HasMaxLength(120).IsRequired();
                li.Property(x => x.UnitPrice).HasPrecision(18, 2);
                li.Property(x => x.LineTotal).HasPrecision(18, 2);
            });
        });
    }
}

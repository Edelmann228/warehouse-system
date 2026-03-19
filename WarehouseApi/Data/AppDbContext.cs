using Microsoft.EntityFrameworkCore;
using WarehouseApi.Models;

namespace WarehouseApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Каждое свойство DbSet<T> = одна таблица в БД
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supply> Supplies => Set<Supply>();
    public DbSet<SupplyItem> SupplyItems => Set<SupplyItem>();
    public DbSet<WriteOff> WriteOffs => Set<WriteOff>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Артикул товара должен быть уникальным
        modelBuilder.Entity<Product>(e => {
            e.HasIndex(p => p.SKU).IsUnique();
            e.Property(p => p.Price).HasPrecision(18, 2);
        });

        // SupplyItem связан с Supply (каскадное удаление) и с Product (ограничение)
        modelBuilder.Entity<SupplyItem>(e => {
            e.HasOne(si => si.Supply)
             .WithMany(s => s.Items)
             .HasForeignKey(si => si.SupplyId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(si => si.Product)
             .WithMany(p => p.SupplyItems)
             .HasForeignKey(si => si.ProductId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(si => si.UnitPrice).HasPrecision(18, 2);
        });

        // WriteOff связан с Product (нельзя удалить товар, если есть списания)
        modelBuilder.Entity<WriteOff>(e => {
            e.HasOne(w => w.Product)
             .WithMany(p => p.WriteOffs)
             .HasForeignKey(w => w.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

using WarehouseApi.Models;

namespace WarehouseApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        // Если данные уже есть — ничего не делаем
        if (db.Products.Any()) return;

        var products = new List<Product>
        {
            new() { Name="Болт М10",      SKU="BOLT-M10",  Unit="шт", Price=2.50m,  StockQuantity=500 },
            new() { Name="Гайка М10",     SKU="NUT-M10",   Unit="шт", Price=1.20m,  StockQuantity=300 },
            new() { Name="Краска белая",  SKU="PAINT-W1",  Unit="л",  Price=180m,   StockQuantity=50  },
            new() { Name="Кабель 2.5мм²", SKU="CABLE-25",  Unit="м",  Price=35m,    StockQuantity=200 },
        };
        db.Products.AddRange(products);
        db.SaveChanges();

        // Поставка с двумя позициями
        db.Supplies.Add(new Supply
        {
            SupplierName = "ООО ТехСнаб",
            SupplyDate = DateTime.UtcNow.AddDays(-10),
            Status = "Accepted",
            Items = new List<SupplyItem> {
                new() { ProductId = products[0].Id, Quantity=200, UnitPrice=2.30m },
                new() { ProductId = products[1].Id, Quantity=100, UnitPrice=1.10m },
            }
        });

        // Одно списание
        db.WriteOffs.Add(new WriteOff
        {
            ProductId = products[2].Id,
            Quantity = 5,
            Reason = "Брак при приёмке",
            WrittenOffAt = DateTime.UtcNow.AddDays(-2)
        });

        db.SaveChanges();
    }
}

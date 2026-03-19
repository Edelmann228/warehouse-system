namespace WarehouseApi.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;       // Название
    public string SKU { get; set; } = string.Empty;       // Артикул
    public string Unit { get; set; } = string.Empty;       // Ед. измерения (шт, кг, м)
    public decimal Price { get; set; }                     // Цена за единицу
    public int StockQuantity { get; set; }                 // Текущий остаток
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Связанные записи (EF Core заполняет автоматически)
    public ICollection<SupplyItem> SupplyItems { get; set; } = new List<SupplyItem>();
    public ICollection<WriteOff> WriteOffs { get; set; } = new List<WriteOff>();
}


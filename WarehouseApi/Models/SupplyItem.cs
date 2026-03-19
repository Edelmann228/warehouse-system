namespace WarehouseApi.Models;

public class SupplyItem
{
    public int Id { get; set; }
    public int SupplyId { get; set; }     // Внешний ключ на Supply
    public int ProductId { get; set; }     // Внешний ключ на Product
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Supply Supply { get; set; } = null!;
    public Product Product { get; set; } = null!;
}


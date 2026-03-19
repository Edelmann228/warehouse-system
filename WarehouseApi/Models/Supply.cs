namespace WarehouseApi.Models;

public class Supply
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateTime SupplyDate { get; set; } = DateTime.UtcNow;
    // Статус: Pending (ожидает), Accepted (принята), Cancelled (отменена)
    public string Status { get; set; } = "Pending";

    public ICollection<SupplyItem> Items { get; set; } = new List<SupplyItem>();
}

namespace WarehouseApi.Models;

public class WriteOff
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;  // Причина списания
    public DateTime WrittenOffAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}

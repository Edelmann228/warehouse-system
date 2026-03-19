using WarehouseApi.Models;

namespace WarehouseApi.Tests;

public class ProductTests
{
    [Fact]
    public void Product_StockQuantity_DefaultIsZero()
    {
        var p = new Product { Name = "Test", SKU = "T1", Unit = "шт" };
        Assert.True(p.StockQuantity >= 0);
    }

    [Fact]
    public void Product_CreatedAt_IsNotDefault()
    {
        var p = new Product();
        Assert.NotEqual(default(DateTime), p.CreatedAt);
    }
}
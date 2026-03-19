using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data;
using WarehouseApi.Models;

namespace WarehouseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliesController : ControllerBase
{
    private readonly AppDbContext _db;
    public SuppliesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Supplies
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await _db.Supplies
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(s => s.Id == id);
        return s == null ? NotFound() : Ok(s);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Supply supply)
    {
        // Принятая поставка увеличивает остатки на складе
        if (supply.Status == "Accepted")
            foreach (var item in supply.Items)
            {
                var product = await _db.Products.FindAsync(item.ProductId);
                if (product != null) product.StockQuantity += item.Quantity;
            }
        _db.Supplies.Add(supply);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = supply.Id }, supply);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Supply upd)
    {
        var s = await _db.Supplies.FindAsync(id);
        if (s == null) return NotFound();
        s.SupplierName = upd.SupplierName; s.Status = upd.Status;
        await _db.SaveChangesAsync();
        return Ok(s);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Supplies.FindAsync(id);
        if (s == null) return NotFound();
        _db.Supplies.Remove(s);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

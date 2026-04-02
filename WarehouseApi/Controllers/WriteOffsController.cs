using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data;
using WarehouseApi.Models;

namespace WarehouseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WriteOffsController : ControllerBase
{
    private readonly AppDbContext _db;

    public WriteOffsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var writeoffs = await _db.WriteOffs
            .Include(w => w.Product)
            .ToListAsync();

        var result = writeoffs.Select(w => new
        {
            w.Id,
            w.ProductId,
            ProductName = w.Product?.Name ?? "—",
            w.Quantity,
            w.Reason,
            w.WrittenOffAt
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var w = await _db.WriteOffs
            .Include(w => w.Product)
            .FirstOrDefaultAsync(w => w.Id == id);

        return w == null ? NotFound() : Ok(w);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WriteOff writeOff)
    {
        var product = await _db.Products.FindAsync(writeOff.ProductId);
        if (product == null) return BadRequest("Товар не найден");
        if (product.StockQuantity < writeOff.Quantity)
            return BadRequest("Недостаточно на складе");

        product.StockQuantity -= writeOff.Quantity;
        _db.WriteOffs.Add(writeOff);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = writeOff.Id }, writeOff);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var w = await _db.WriteOffs.FindAsync(id);
        if (w == null) return NotFound();

        _db.WriteOffs.Remove(w);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
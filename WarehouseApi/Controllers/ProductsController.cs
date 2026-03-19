using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApi.Data;
using WarehouseApi.Models;
using WarehouseApi.Services;

namespace WarehouseApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CacheService _cache;
    private const string AllKey = "products:all";

    public ProductsController(AppDbContext db, CacheService cache)
    { _db = db; _cache = cache; }

    // GET api/products  →  сначала проверяем кэш, потом БД
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cached = await _cache.GetAsync<List<Product>>(AllKey);
        if (cached != null) return Ok(cached);

        var list = await _db.Products.ToListAsync();
        await _cache.SetAsync(AllKey, list, TimeSpan.FromMinutes(5));
        return Ok(list);
    }

    // GET api/products/3  →  кэш по ключу "product:3"
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var key = $"product:{id}";
        var cached = await _cache.GetAsync<Product>(key);
        if (cached != null) return Ok(cached);

        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();
        await _cache.SetAsync(key, product, TimeSpan.FromMinutes(5));
        return Ok(product);
    }

    // POST api/products  →  создать и инвалидировать кэш списка
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync(AllKey);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT api/products/3  →  обновить и инвалидировать оба кэша
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product upd)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();
        p.Name = upd.Name; p.Price = upd.Price;
        p.StockQuantity = upd.StockQuantity; p.Unit = upd.Unit;
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync(AllKey);
        await _cache.RemoveAsync($"product:{id}");
        return Ok(p);
    }

    // DELETE api/products/3
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();
        _db.Products.Remove(p);
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync(AllKey);
        await _cache.RemoveAsync($"product:{id}");
        return NoContent();
    }
}

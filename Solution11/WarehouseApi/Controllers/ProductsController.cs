using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

namespace WarehouseApi.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetAll(
        [FromQuery] int? categoryId, [FromQuery] string? search, CancellationToken ct) =>
        Ok(await products.GetAllAsync(categoryId, search, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(id, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product product, CancellationToken ct)
    {
        try
        {
            var created = await products.CreateAsync(product, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.ProductId }, created);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Product>> Update(int id, Product product, CancellationToken ct)
    {
        try
        {
            var updated = await products.UpdateAsync(id, product, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try { return await products.DeleteAsync(id, ct) ? NoContent() : NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }
}

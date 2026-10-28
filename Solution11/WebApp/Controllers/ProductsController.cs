using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
namespace WebApplication1.Controllers;

using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service) => _service = service;

    // GET api/products?categoryId=1&search=lap
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetAll(
        [FromQuery] int? categoryId, [FromQuery] string? search, CancellationToken ct)
        => Ok(await _service.GetAllAsync(categoryId, search, ct));

    // GET api/products/5
    [HttpGet("{id:int}", Name = "GetProduct")]
    public async Task<ActionResult<Product>> GetById(int id, CancellationToken ct)
    {
        var product = await _service.GetByIdAsync(id, ct);
        return product is null ? NotFound() : Ok(product);
    }

    // POST api/products   body: { "name": "Laptop", "price": 999.99, "categoryId": 1 }
    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product product, CancellationToken ct)
    {
        try
        {
            var created = await _service.CreateAsync(product, ct);
            return CreatedAtRoute("GetProduct", new { id = created.ProductId }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // PUT api/products/5
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Product>> Update(int id, Product product, CancellationToken ct)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, product, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // DELETE api/products/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            return await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
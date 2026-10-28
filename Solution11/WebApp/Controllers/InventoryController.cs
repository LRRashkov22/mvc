using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service) => _service = service;

    // GET api/inventory?productId=1&warehouseId=2
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InventoryItem>>> GetAll(
        [FromQuery] int? productId, [FromQuery] int? warehouseId, CancellationToken ct)
        => Ok(await _service.GetAllAsync(productId, warehouseId, ct));

    // GET api/inventory/1/2
    [HttpGet("{productId:int}/{warehouseId:int}", Name = "GetInventoryItem")]
    public async Task<ActionResult<InventoryItem>> GetById(int productId, int warehouseId, CancellationToken ct)
    {
        var item = await _service.GetByIdAsync(productId, warehouseId, ct);
        return item is null ? NotFound() : Ok(item);
    }

    // POST api/inventory   body: { "productId": 1, "warehouseId": 2, "quantity": 10 }
    [HttpPost]
    public async Task<ActionResult<InventoryItem>> Create(InventoryItem item, CancellationToken ct)
    {
        try
        {
            var created = await _service.CreateAsync(item, ct);
            return CreatedAtRoute("GetInventoryItem",
                new { productId = created.ProductId, warehouseId = created.WarehouseId }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // PUT api/inventory/1/2   body: { "quantity": 25 }  — sets absolute quantity (ids come from the route)
    [HttpPut("{productId:int}/{warehouseId:int}")]
    public async Task<ActionResult<InventoryItem>> UpdateQuantity(
        int productId, int warehouseId, InventoryItem item, CancellationToken ct)
    {
        var updated = await _service.UpdateQuantityAsync(productId, warehouseId, item.Quantity, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    // PATCH api/inventory/1/2/adjust?delta=-5  — add/remove stock
    [HttpPatch("{productId:int}/{warehouseId:int}/adjust")]
    public async Task<ActionResult<InventoryItem>> Adjust(
        int productId, int warehouseId, [FromQuery] int delta, CancellationToken ct)
    {
        try
        {
            var updated = await _service.AdjustQuantityAsync(productId, warehouseId, delta, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE api/inventory/1/2
    [HttpDelete("{productId:int}/{warehouseId:int}")]
    public async Task<IActionResult> Delete(int productId, int warehouseId, CancellationToken ct)
        => await _service.DeleteAsync(productId, warehouseId, ct) ? NoContent() : NotFound();
}
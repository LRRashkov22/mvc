using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
namespace WebApplication1.Controllers;

using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _service;

    public WarehousesController(IWarehouseService service)
    {
        _service = service;
    }

    // GET: api/warehouses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Warehouse>>> GetAll(CancellationToken ct)
    {
        return Ok(await _service.GetAllAsync(ct));
    }

    // GET: api/warehouses/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Warehouse>> GetById(int id, CancellationToken ct)
    {
        var warehouse = await _service.GetByIdAsync(id, ct);
        return warehouse is null ? NotFound() : Ok(warehouse);
    }

    // POST: api/warehouses
    [HttpPost]
    public async Task<ActionResult<Warehouse>> Create(Warehouse warehouse, CancellationToken ct)
    {
        var created = await _service.CreateAsync(warehouse, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.WarehouseId }, created);
    }

    // PUT: api/warehouses/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Warehouse warehouse, CancellationToken ct)
    {
        if (warehouse.WarehouseId != 0 && warehouse.WarehouseId != id)
            return BadRequest(new { message = "Route id and body WarehouseId do not match." });

        var updated = await _service.UpdateAsync(id, warehouse, ct);
        return updated ? NoContent() : NotFound();
    }

    // DELETE: api/warehouses/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(id, ct);

        return result switch
        {
            DeleteWarehouseResult.Deleted => NoContent(),
            DeleteWarehouseResult.NotFound => NotFound(),
            DeleteWarehouseResult.HasRelatedData => Conflict(new
            {
                message = "Cannot delete a warehouse that has inventory items or deliveries."
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Controllers;

[Authorize]
public class WarehousePagesController : Controller
{
    private readonly IWarehouseService _warehouses;

    public WarehousePagesController(IWarehouseService warehouses)
    {
        _warehouses = warehouses;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _warehouses.GetAllAsync());
    }

    public IActionResult Create() => View(new Warehouse());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name")] Warehouse warehouse)
    {
        if (!ModelState.IsValid) return View(warehouse);

        await _warehouses.CreateAsync(warehouse);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var warehouse = await _warehouses.GetByIdAsync(id.Value);
        return warehouse is null ? NotFound() : View(warehouse);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("WarehouseId,Name")] Warehouse warehouse)
    {
        if (id != warehouse.WarehouseId) return NotFound();
        if (!ModelState.IsValid) return View(warehouse);

        var updated = await _warehouses.UpdateAsync(id, warehouse);
        if (!updated) return NotFound();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var warehouse = await _warehouses.GetByIdAsync(id.Value);
        return warehouse is null ? NotFound() : View(warehouse);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _warehouses.DeleteAsync(id);
        if (result == DeleteWarehouseResult.NotFound) return NotFound();

        if (result == DeleteWarehouseResult.HasRelatedData)
        {
            TempData["Error"] = "This warehouse has inventory or delivery records and cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }
}

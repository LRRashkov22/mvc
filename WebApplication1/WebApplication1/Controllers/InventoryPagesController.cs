using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Controllers;

public class InventoryPagesController : Controller
{
    private readonly WarehouseDbContext _db;
    private readonly IInventoryService _inventory;

    public InventoryPagesController(WarehouseDbContext db, IInventoryService inventory)
    {
        _db = db;
        _inventory = inventory;
    }

    public async Task<IActionResult> Index(string? search, int? warehouseId)
    {
        ViewBag.Search = search;
        ViewBag.WarehouseId = warehouseId;
        ViewBag.Warehouses = new SelectList(await _db.Warehouses.AsNoTracking().OrderBy(x => x.Name).ToListAsync(), "WarehouseId", "Name", warehouseId);
        var query = _db.Inventory.AsNoTracking().Include(x => x.Product).ThenInclude(x => x!.Category)
            .Include(x => x.Warehouse).AsQueryable();
        if (warehouseId.HasValue) query = query.Where(x => x.WarehouseId == warehouseId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Product != null && x.Product.Name.Contains(search));
        return View(await query.OrderBy(x => x.Product!.Name).ToListAsync());
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View(new InventoryItem());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InventoryItem item)
    {
        if (!ModelState.IsValid) { await PopulateDropdowns(item.ProductId, item.WarehouseId); return View(item); }
        try
        {
            await _inventory.CreateAsync(item);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropdowns(item.ProductId, item.WarehouseId);
            return View(item);
        }
    }

    public async Task<IActionResult> Edit(int productId, int warehouseId)
    {
        var item = await _inventory.GetByIdAsync(productId, warehouseId);
        if (item is null) return NotFound();
        await PopulateDropdowns(item.ProductId, item.WarehouseId);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int productId, int warehouseId, InventoryItem item)
    {
        if (!ModelState.IsValid) { await PopulateDropdowns(item.ProductId, item.WarehouseId); return View(item); }
        var updated = await _inventory.UpdateQuantityAsync(productId, warehouseId, item.Quantity);
        if (updated is null) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust(int productId, int warehouseId, int delta)
    {
        try
        {
            var updated = await _inventory.AdjustQuantityAsync(productId, warehouseId, delta);
            if (updated is null) return NotFound();
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(int? productId = null, int? warehouseId = null)
    {
        ViewBag.Products = new SelectList(await _db.Products.AsNoTracking().OrderBy(x => x.Name).ToListAsync(), "ProductId", "Name", productId);
        ViewBag.Warehouses = new SelectList(await _db.Warehouses.AsNoTracking().OrderBy(x => x.Name).ToListAsync(), "WarehouseId", "Name", warehouseId);
    }
}

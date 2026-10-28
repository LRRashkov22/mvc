using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Controllers;

[Authorize]
public class InventoryPagesController : Controller
{
    private readonly IInventoryService _inventory;
    private readonly IProductService _products;
    private readonly IWarehouseService _warehouses;

    public InventoryPagesController(
        IInventoryService inventory,
        IProductService products,
        IWarehouseService warehouses)
    {
        _inventory = inventory;
        _products = products;
        _warehouses = warehouses;
    }

    public async Task<IActionResult> Index(int? warehouseId, string? search)
    {
        var items = await _inventory.GetAllAsync(warehouseId: warehouseId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var products = await _products.GetAllAsync(search: term);
            var matchingProductIds = products.Select(p => p.ProductId).ToHashSet();
            items = items.Where(item => matchingProductIds.Contains(item.ProductId)).ToList();
        }

        ViewBag.Search = search;
        await PopulateDropdownsAsync(selectedWarehouseId: warehouseId);
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View(new InventoryItem());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ProductId,WarehouseId,Quantity")] InventoryItem item)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(item.ProductId, item.WarehouseId);
            return View(item);
        }

        try
        {
            await _inventory.CreateAsync(item);
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await PopulateDropdownsAsync(item.ProductId, item.WarehouseId);
        return View(item);
    }

    public async Task<IActionResult> Edit(int? productId, int? warehouseId)
    {
        if (productId is null || warehouseId is null) return NotFound();

        var item = await _inventory.GetByIdAsync(productId.Value, warehouseId.Value);
        if (item is null) return NotFound();

        await PopulateDropdownsAsync(item.ProductId, item.WarehouseId);
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int productId,
        int warehouseId,
        [Bind("ProductId,WarehouseId,Quantity")] InventoryItem item)
    {
        if (productId != item.ProductId || warehouseId != item.WarehouseId)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(item.ProductId, item.WarehouseId);
            return View(item);
        }

        try
        {
            var updated = await _inventory.UpdateQuantityAsync(productId, warehouseId, item.Quantity);
            if (updated is null) return NotFound();
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ModelState.AddModelError(nameof(item.Quantity), ex.Message);
            await PopulateDropdownsAsync(item.ProductId, item.WarehouseId);
            return View(item);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust(int productId, int warehouseId, int delta)
    {
        try
        {
            var updated = await _inventory.AdjustQuantityAsync(productId, warehouseId, delta);
            if (updated is null) return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(int? selectedProductId = null, int? selectedWarehouseId = null)
    {
        var products = await _products.GetAllAsync();
        var warehouses = await _warehouses.GetAllAsync();

        ViewBag.Products = new SelectList(products, "ProductId", "Name", selectedProductId);
        ViewBag.Warehouses = new SelectList(warehouses, "WarehouseId", "Name", selectedWarehouseId);
    }
}

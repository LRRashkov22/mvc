using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Controllers;

public class ProductPagesController : Controller
{
    private readonly IProductService _products;
    private readonly WarehouseDbContext _db;

    public ProductPagesController(IProductService products, WarehouseDbContext db)
    {
        _products = products;
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId)
    {
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = new SelectList(await _db.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync(), "CategoryId", "Name", categoryId);
        var products = await _products.GetAllAsync(categoryId, search);
        var categories = await _db.Categories.AsNoTracking().ToDictionaryAsync(x => x.CategoryId, x => x.Name);
        ViewBag.CategoryNames = categories;
        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateCategories();
        return View(new Product());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (!ModelState.IsValid) { await PopulateCategories(product.CategoryId); return View(product); }
        try { await _products.CreateAsync(product); return RedirectToAction(nameof(Index)); }
        catch (KeyNotFoundException ex) { ModelState.AddModelError(nameof(product.CategoryId), ex.Message); }
        await PopulateCategories(product.CategoryId);
        return View(product);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product is null) return NotFound();
        await PopulateCategories(product.CategoryId);
        return View(product);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.ProductId) return NotFound();
        if (!ModelState.IsValid) { await PopulateCategories(product.CategoryId); return View(product); }
        try
        {
            var updated = await _products.UpdateAsync(id, product);
            if (updated is null) return NotFound();
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException ex) { ModelState.AddModelError(nameof(product.CategoryId), ex.Message); }
        await PopulateCategories(product.CategoryId);
        return View(product);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _products.GetByIdAsync(id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var deleted = await _products.DeleteAsync(id);
            if (!deleted) return NotFound();
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task PopulateCategories(int? selectedId = null)
    {
        ViewBag.Categories = new SelectList(
            await _db.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync(),
            "CategoryId", "Name", selectedId);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Controllers;

[Authorize]
public class ProductPagesController : Controller
{
    private readonly IProductService _products;
    private readonly ICategoriesService _categories;

    public ProductPagesController(IProductService products, ICategoriesService categories)
    {
        _products = products;
        _categories = categories;
    }

    public async Task<IActionResult> Index(int? categoryId, string? search)
    {
        var products = await _products.GetAllAsync(categoryId, search);
        var categories = await _categories.GetAllAsync();

        ViewBag.Search = search;
        ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", categoryId);
        ViewBag.CategoryNames = categories.ToDictionary(c => c.CategoryId, c => c.Name);

        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Price,CategoryId")] Product product)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(product.CategoryId);
            return View(product);
        }

        try
        {
            await _products.CreateAsync(product);
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException ex)
        {
            ModelState.AddModelError(nameof(product.CategoryId), ex.Message);
            await PopulateCategoriesAsync(product.CategoryId);
            return View(product);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var product = await _products.GetByIdAsync(id.Value);
        if (product is null) return NotFound();

        await PopulateCategoriesAsync(product.CategoryId);
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,Price,CategoryId")] Product product)
    {
        if (id != product.ProductId) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(product.CategoryId);
            return View(product);
        }

        try
        {
            var updated = await _products.UpdateAsync(id, product);
            if (updated is null) return NotFound();
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException ex)
        {
            ModelState.AddModelError(nameof(product.CategoryId), ex.Message);
            await PopulateCategoriesAsync(product.CategoryId);
            return View(product);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var product = await _products.GetByIdAsync(id.Value);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
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

    private async Task PopulateCategoriesAsync(int? selectedCategoryId = null)
    {
        var categories = await _categories.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", selectedCategoryId);
    }
}

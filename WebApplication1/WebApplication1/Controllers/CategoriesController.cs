using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Controllers;

public class CategoriesController : Controller
{
    private readonly ICategoriesService _categoriesService;

    public CategoriesController(ICategoriesService categoriesService)
    {
        _categoriesService = categoriesService;
    }

    // GET: Categories
    public async Task<IActionResult> Index()
    {
        var categories = await _categoriesService.GetAllAsync();
        return View(categories);
    }

    // GET: Categories/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var category = await _categoriesService.GetByIdAsync(id.Value);

        if (category == null)
            return NotFound();

        return View(category);
    }

    // GET: Categories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Name")] Category category)
    {
        if (!ModelState.IsValid)
            return View(category);

        await _categoriesService.CreateAsync(category);

        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var category = await _categoriesService.GetByIdAsync(id.Value);

        if (category == null)
            return NotFound();

        return View(category);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("CategoryId,Name")] Category category)
    {
        if (id != category.CategoryId)
            return NotFound();

        if (!ModelState.IsValid)
            return View(category);

        var existingCategory = await _categoriesService.GetByIdAsync(id);

        if (existingCategory == null)
            return NotFound();

        existingCategory.Name = category.Name;

        await _categoriesService.UpdateAsync(existingCategory);

        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var category = await _categoriesService.GetByIdAsync(id.Value);

        if (category == null)
            return NotFound();

        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _categoriesService.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        await _categoriesService.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WarehouseInventoryMvc.Data;
using WarehouseInventoryMvc.Models;

namespace WarehouseInventoryMvc.Controllers;

public class InventoryController : Controller
{
    private readonly WarehouseDbContext _db;

    public InventoryController(WarehouseDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Products = await _db.Products.CountAsync();
        ViewBag.Categories = await _db.Categories.CountAsync();
        ViewBag.Warehouses = await _db.Warehouses.CountAsync();
        ViewBag.Units = await _db.Inventory.SumAsync(x => (int?)x.Quantity) ?? 0;
        ViewBag.Stock = await _db.Inventory
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .OrderBy(x => x.Quantity)
            .Take(10)
            .ToListAsync();

        return View("~/Views/Home/Index.cshtml");
    }

    // Categories
    public async Task<IActionResult> Categories()
    {
        return View("~/Views/Categories/Index.cshtml",
            await _db.Categories.OrderBy(x => x.Name).ToListAsync());
    }

    public IActionResult CreateCategory()
    {
        return View("~/Views/Categories/Create.cshtml", new Category());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(Category category)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Categories/Create.cshtml", category);

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Categories));
    }

    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        return category == null
            ? NotFound()
            : View("~/Views/Categories/Delete.cshtml", category);
    }

    [HttpPost, ActionName("DeleteCategory")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategoryConfirmed(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category != null)
        {
            _db.Categories.Remove(category);
            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Категорията се използва от продукт.";
            }
        }
        return RedirectToAction(nameof(Categories));
    }

    // Products
    public async Task<IActionResult> Products()
    {
        var products = await _db.Products
            .Include(x => x.Category)
            .OrderBy(x => x.Name)
            .ToListAsync();
        return View("~/Views/Products/Index.cshtml", products);
    }

    public async Task<IActionResult> CreateProduct()
    {
        await LoadCategories();
        return View("~/Views/Products/Create.cshtml", new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(Product product)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategories(product.CategoryId);
            return View("~/Views/Products/Create.cshtml", product);
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Products));
    }

    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.ProductId == id);
        return product == null
            ? NotFound()
            : View("~/Views/Products/Delete.cshtml", product);
    }

    [HttpPost, ActionName("DeleteProduct")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProductConfirmed(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            _db.Products.Remove(product);
            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Продуктът се използва в наличности или доставки.";
            }
        }
        return RedirectToAction(nameof(Products));
    }

    // Warehouses
    public async Task<IActionResult> Warehouses()
    {
        return View("~/Views/Warehouses/Index.cshtml",
            await _db.Warehouses.OrderBy(x => x.Name).ToListAsync());
    }

    public IActionResult CreateWarehouse()
    {
        return View("~/Views/Warehouses/Create.cshtml", new Warehouse());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWarehouse(Warehouse warehouse)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Warehouses/Create.cshtml", warehouse);

        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Warehouses));
    }

    // Inventory
    public async Task<IActionResult> Stock()
    {
        var stock = await _db.Inventory
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .OrderBy(x => x.Product!.Name)
            .ToListAsync();
        return View("~/Views/Inventory/Index.cshtml", stock);
    }

    public async Task<IActionResult> CreateStock()
    {
        await LoadProductsAndWarehouses();
        return View("~/Views/Inventory/Create.cshtml", new InventoryItem());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStock(InventoryItem item)
    {
        if (!ModelState.IsValid)
        {
            await LoadProductsAndWarehouses(item.ProductId, item.WarehouseId);
            return View("~/Views/Inventory/Create.cshtml", item);
        }

        var existing = await _db.Inventory.FindAsync(item.ProductId, item.WarehouseId);
        if (existing == null)
            _db.Inventory.Add(item);
        else
            existing.Quantity = item.Quantity;

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Stock));
    }

    // Deliveries
    public async Task<IActionResult> Deliveries()
    {
        var deliveries = await _db.Deliveries
            .Include(x => x.Product)
            .Include(x => x.Warehouse)
            .OrderByDescending(x => x.DeliveryDate)
            .ToListAsync();
        return View("~/Views/Deliveries/Index.cshtml", deliveries);
    }

    public async Task<IActionResult> CreateDelivery()
    {
        await LoadProductsAndWarehouses();
        return View("~/Views/Deliveries/Create.cshtml", new Delivery());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDelivery(Delivery delivery)
    {
        if (!ModelState.IsValid)
        {
            await LoadProductsAndWarehouses(delivery.ProductId, delivery.WarehouseId);
            return View("~/Views/Deliveries/Create.cshtml", delivery);
        }

        _db.Deliveries.Add(delivery);
        var stock = await _db.Inventory.FindAsync(delivery.ProductId, delivery.WarehouseId);
        if (stock == null)
        {
            _db.Inventory.Add(new InventoryItem
            {
                ProductId = delivery.ProductId,
                WarehouseId = delivery.WarehouseId,
                Quantity = delivery.Quantity
            });
        }
        else
        {
            stock.Quantity += delivery.Quantity;
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Deliveries));
    }

    private async Task LoadCategories(int? selectedId = null)
    {
        ViewBag.CategoryId = new SelectList(
            await _db.Categories.OrderBy(x => x.Name).ToListAsync(),
            "CategoryId", "Name", selectedId);
    }

    private async Task LoadProductsAndWarehouses(
        int? productId = null, int? warehouseId = null)
    {
        ViewBag.ProductId = new SelectList(
            await _db.Products.OrderBy(x => x.Name).ToListAsync(),
            "ProductId", "Name", productId);
        ViewBag.WarehouseId = new SelectList(
            await _db.Warehouses.OrderBy(x => x.Name).ToListAsync(),
            "WarehouseId", "Name", warehouseId);
    }
}

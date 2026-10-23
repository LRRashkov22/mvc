using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly WarehouseDbContext _db;
    public HomeController(WarehouseDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        ViewBag.ProductCount = await _db.Products.CountAsync();
        ViewBag.CategoryCount = await _db.Categories.CountAsync();
        ViewBag.WarehouseCount = await _db.Warehouses.CountAsync();
        ViewBag.StockUnits = await _db.Inventory.SumAsync(x => (int?)x.Quantity) ?? 0;
        ViewBag.LowStock = await _db.Inventory
            .Include(x => x.Product).Include(x => x.Warehouse)
            .Where(x => x.Quantity <= 5)
            .OrderBy(x => x.Quantity).Take(6).ToListAsync();
        ViewBag.RecentDeliveries = await _db.Deliveries
            .Include(x => x.Product).Include(x => x.Warehouse)
            .OrderByDescending(x => x.DeliveryDate).Take(5).ToListAsync();
        return View();
    }

    public IActionResult Privacy() => View();
}

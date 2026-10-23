using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.Contracts;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IProductService _products;
    private readonly ICategoriesService _categories;
    private readonly IWarehouseService _warehouses;
    private readonly IInventoryService _inventory;
    private readonly IDeliveriesService _deliveries;

    public HomeController(
        IProductService products,
        ICategoriesService categories,
        IWarehouseService warehouses,
        IInventoryService inventory,
        IDeliveriesService deliveries)
    {
        _products = products;
        _categories = categories;
        _warehouses = warehouses;
        _inventory = inventory;
        _deliveries = deliveries;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _products.GetAllAsync();
        var categories = await _categories.GetAllAsync();
        var warehouses = await _warehouses.GetAllAsync();
        var inventory = await _inventory.GetAllAsync();
        var deliveries = await _deliveries.GetAllAsync();

        ViewBag.ProductCount = products.Count;
        ViewBag.CategoryCount = categories.Count();
        ViewBag.WarehouseCount = warehouses.Count();
        ViewBag.StockUnits = inventory.Sum(item => item.Quantity);
        ViewBag.LowStock = inventory.Where(item => item.Quantity <= 5).ToList();
        ViewBag.RecentDeliveries = deliveries.Take(5).ToList();

        return View();
    }

    [AllowAnonymous]
    public IActionResult Error() =>
        View("~/Views/Shared/Error.cshtml",
            new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
}

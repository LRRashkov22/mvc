
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Controllers;

public class DeliveriesController : Controller
{
    private readonly IDeliveriesService _deliveriesService;

    public DeliveriesController(IDeliveriesService deliveriesService)
    {
        _deliveriesService = deliveriesService;
    }

    // GET: Deliveries
    public async Task<IActionResult> Index()
    {
        var deliveries = await _deliveriesService.GetAllAsync();
        return View(deliveries);
    }

    // GET: Deliveries/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var delivery = await _deliveriesService.GetByIdAsync(id.Value);

        if (delivery == null)
            return NotFound();

        return View(delivery);
    }

    // GET: Deliveries/Create
    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View(new Delivery());
    }

    // POST: Deliveries/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("ProductId,WarehouseId,Quantity,DeliveryDate")]
        Delivery delivery)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(
                delivery.ProductId,
                delivery.WarehouseId);

            return View(delivery);
        }

        await _deliveriesService.CreateAsync(delivery);

        return RedirectToAction(nameof(Index));
    }

    // GET: Deliveries/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var delivery = await _deliveriesService.GetByIdAsync(id.Value);

        if (delivery == null)
            return NotFound();

        await PopulateDropdownsAsync(
            delivery.ProductId,
            delivery.WarehouseId);

        return View(delivery);
    }

    // POST: Deliveries/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("DeliveryId,ProductId,WarehouseId,Quantity,DeliveryDate")]
        Delivery delivery)
    {
        if (id != delivery.DeliveryId)
            return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(
                delivery.ProductId,
                delivery.WarehouseId);

            return View(delivery);
        }

        var updated = await _deliveriesService.UpdateAsync(delivery);

        if (!updated)
            return NotFound();

        return RedirectToAction(nameof(Index));
    }

    // GET: Deliveries/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var delivery = await _deliveriesService.GetByIdAsync(id.Value);

        if (delivery == null)
            return NotFound();

        return View(delivery);
    }

    // POST: Deliveries/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await _deliveriesService.ExistsAsync(id))
            return NotFound();

        await _deliveriesService.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(
        int? selectedProductId = null,
        int? selectedWarehouseId = null)
    {
        ViewData["ProductId"] =
            await _deliveriesService.GetProductsSelectListAsync(
                selectedProductId);

        ViewData["WarehouseId"] =
            await _deliveriesService.GetWarehousesSelectListAsync(
                selectedWarehouseId);
    }
}

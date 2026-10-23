using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Services.Implementations;

public sealed class DeliveriesService : IDeliveriesService
{
    private readonly IDeliveryRepository _deliveries;
    public DeliveriesService(IDeliveryRepository deliveries) => _deliveries = deliveries;

    public async Task<IEnumerable<Delivery>> GetAllAsync() => await _deliveries.GetAllAsync();

    public Task<Delivery?> GetByIdAsync(int id) => _deliveries.GetByIdAsync(id);

    public async Task CreateAsync(Delivery delivery)
    {
        if (delivery.Quantity <= 0) throw new ArgumentOutOfRangeException(nameof(delivery.Quantity));
        delivery.Product = null;
        delivery.Warehouse = null;
        await _deliveries.AddAsync(delivery);
        await _deliveries.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Delivery delivery)
    {
        var existing = await _deliveries.GetTrackedByIdAsync(delivery.DeliveryId);
        if (existing is null) return false;
        existing.ProductId = delivery.ProductId;
        existing.WarehouseId = delivery.WarehouseId;
        existing.Quantity = delivery.Quantity;
        existing.DeliveryDate = delivery.DeliveryDate;
        await _deliveries.SaveChangesAsync();
        return true;
    }

    public async Task DeleteAsync(int id)
    {
        var delivery = await _deliveries.GetTrackedByIdAsync(id);
        if (delivery is null) return;
        await _deliveries.DeleteAsync(delivery);
        await _deliveries.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(int id) => _deliveries.ExistsAsync(id);

    public async Task<SelectList> GetProductsSelectListAsync(int? selectedProductId = null)
    {
        var products = await _deliveries.GetProductsAsync();
        return new SelectList(products, "ProductId", "Name", selectedProductId);
    }

    public async Task<SelectList> GetWarehousesSelectListAsync(int? selectedWarehouseId = null)
    {
        var warehouses = await _deliveries.GetWarehousesAsync();
        return new SelectList(warehouses, "WarehouseId", "Name", selectedWarehouseId);
    }
}

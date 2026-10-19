namespace WebApplication1.Services.Implementations;


using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

public class DeliveriesService : IDeliveriesService
{
    private readonly WarehouseDbContext _context;

    public DeliveriesService(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Delivery>> GetAllAsync()
    {
        return await _context.Deliveries
            .AsNoTracking()
            .Include(d => d.Product)
            .Include(d => d.Warehouse)
            .ToListAsync();
    }

    public async Task<Delivery?> GetByIdAsync(int id)
    {
        return await _context.Deliveries
            .AsNoTracking()
            .Include(d => d.Product)
            .Include(d => d.Warehouse)
            .FirstOrDefaultAsync(d => d.DeliveryId == id);
    }

    public async Task CreateAsync(Delivery delivery)
    {
        await _context.Deliveries.AddAsync(delivery);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Delivery delivery)
    {
        var existingDelivery = await _context.Deliveries
            .FindAsync(delivery.DeliveryId);

        if (existingDelivery == null)
            return false;

        existingDelivery.ProductId = delivery.ProductId;
        existingDelivery.WarehouseId = delivery.WarehouseId;
        existingDelivery.Quantity = delivery.Quantity;
        existingDelivery.DeliveryDate = delivery.DeliveryDate;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task DeleteAsync(int id)
    {
        var delivery = await _context.Deliveries.FindAsync(id);

        if (delivery == null)
            return;

        _context.Deliveries.Remove(delivery);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Deliveries
            .AnyAsync(d => d.DeliveryId == id);
    }

    public async Task<SelectList> GetProductsSelectListAsync(
        int? selectedProductId = null)
    {
        var products = await _context.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        return new SelectList(
            products,
            "ProductId",
            "Name",
            selectedProductId);
    }

    public async Task<SelectList> GetWarehousesSelectListAsync(
        int? selectedWarehouseId = null)
    {
        var warehouses = await _context.Warehouses
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .ToListAsync();

        return new SelectList(
            warehouses,
            "WarehouseId",
            "Name",
            selectedWarehouseId);
    }
}

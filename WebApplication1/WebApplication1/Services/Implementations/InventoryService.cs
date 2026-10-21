namespace WebApplication1.Services.Implementations;

using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

public class InventoryService : IInventoryService
{
    private readonly WarehouseDbContext _db;

    public InventoryService(WarehouseDbContext db) => _db = db;

    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(
        int? productId = null, int? warehouseId = null, CancellationToken ct = default)
    {
        var query = _db.Inventory.AsNoTracking().AsQueryable();

        if (productId.HasValue)
            query = query.Where(x => x.ProductId == productId.Value);
        if (warehouseId.HasValue)
            query = query.Where(x => x.WarehouseId == warehouseId.Value);

        return await query
            .OrderBy(x => x.WarehouseId).ThenBy(x => x.ProductId)
            .ToListAsync(ct);
    }

    public async Task<InventoryItem?> GetByIdAsync(int productId, int warehouseId, CancellationToken ct = default)
    {
        return await _db.Inventory.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.WarehouseId == warehouseId, ct);
    }

    public async Task<InventoryItem> CreateAsync(InventoryItem item, CancellationToken ct = default)
    {
        if (!await _db.Products.AnyAsync(p => p.ProductId == item.ProductId, ct))
            throw new KeyNotFoundException($"Product {item.ProductId} not found.");

        if (!await _db.Warehouses.AnyAsync(w => w.WarehouseId == item.WarehouseId, ct))
            throw new KeyNotFoundException($"Warehouse {item.WarehouseId} not found.");

        if (await _db.Inventory.AnyAsync(i => i.ProductId == item.ProductId && i.WarehouseId == item.WarehouseId, ct))
            throw new InvalidOperationException("Inventory record for this product and warehouse already exists.");

        // Don't let clients insert/modify related entities through the request body
        item.Product = null;
        item.Warehouse = null;

        _db.Inventory.Add(item);
        await _db.SaveChangesAsync(ct);
        return item;
    }

    public async Task<InventoryItem?> UpdateQuantityAsync(
        int productId, int warehouseId, int quantity, CancellationToken ct = default)
    {
        var item = await _db.Inventory.FindAsync(new object[] { productId, warehouseId }, ct);
        if (item is null) return null;

        item.Quantity = quantity;
        await _db.SaveChangesAsync(ct);
        return item;
    }

    public async Task<InventoryItem?> AdjustQuantityAsync(
        int productId, int warehouseId, int delta, CancellationToken ct = default)
    {
        var item = await _db.Inventory.FindAsync(new object[] { productId, warehouseId }, ct);
        if (item is null) return null;

        var newQuantity = (long)item.Quantity + delta;
        if (newQuantity < 0)
            throw new InvalidOperationException($"Insufficient stock. Current quantity: {item.Quantity}.");
        if (newQuantity > int.MaxValue)
            throw new InvalidOperationException("Resulting quantity is too large.");

        item.Quantity = (int)newQuantity;
        await _db.SaveChangesAsync(ct);
        return item;
    }

    public async Task<bool> DeleteAsync(int productId, int warehouseId, CancellationToken ct = default)
    {
        var item = await _db.Inventory.FindAsync(new object[] { productId, warehouseId }, ct);
        if (item is null) return false;

        _db.Inventory.Remove(item);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
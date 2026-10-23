using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Services.Implementations;

public sealed class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventory;
    public InventoryService(IInventoryRepository inventory) => _inventory = inventory;

    public Task<IReadOnlyList<InventoryItem>> GetAllAsync(int? productId = null, int? warehouseId = null, CancellationToken ct = default) =>
        _inventory.GetAllAsync(productId, warehouseId, ct);

    public Task<InventoryItem?> GetByIdAsync(int productId, int warehouseId, CancellationToken ct = default) =>
        _inventory.GetByIdAsync(productId, warehouseId, ct);

    public async Task<InventoryItem> CreateAsync(InventoryItem item, CancellationToken ct = default)
    {
        if (!await _inventory.ProductExistsAsync(item.ProductId, ct))
            throw new KeyNotFoundException($"Product {item.ProductId} not found.");
        if (!await _inventory.WarehouseExistsAsync(item.WarehouseId, ct))
            throw new KeyNotFoundException($"Warehouse {item.WarehouseId} not found.");
        if (await _inventory.ExistsAsync(item.ProductId, item.WarehouseId, ct))
            throw new InvalidOperationException("Inventory record for this product and warehouse already exists.");

        item.Product = null;
        item.Warehouse = null;
        await _inventory.AddAsync(item, ct);
        await _inventory.SaveChangesAsync(ct);
        return item;
    }

    public async Task<InventoryItem?> UpdateQuantityAsync(int productId, int warehouseId, int quantity, CancellationToken ct = default)
    {
        var item = await _inventory.GetTrackedByIdAsync(productId, warehouseId, ct);
        if (item is null) return null;
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");
        item.Quantity = quantity;
        await _inventory.SaveChangesAsync(ct);
        return item;
    }

    public async Task<InventoryItem?> AdjustQuantityAsync(int productId, int warehouseId, int delta, CancellationToken ct = default)
    {
        var item = await _inventory.GetTrackedByIdAsync(productId, warehouseId, ct);
        if (item is null) return null;
        var newQuantity = (long)item.Quantity + delta;
        if (newQuantity < 0) throw new InvalidOperationException($"Insufficient stock. Current quantity: {item.Quantity}.");
        if (newQuantity > int.MaxValue) throw new InvalidOperationException("Resulting quantity is too large.");
        item.Quantity = (int)newQuantity;
        await _inventory.SaveChangesAsync(ct);
        return item;
    }

    public async Task<bool> DeleteAsync(int productId, int warehouseId, CancellationToken ct = default)
    {
        var item = await _inventory.GetTrackedByIdAsync(productId, warehouseId, ct);
        if (item is null) return false;
        await _inventory.DeleteAsync(item, ct);
        await _inventory.SaveChangesAsync(ct);
        return true;
    }
}

using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;

namespace WebApplication1.Repositories.Implementations;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly WarehouseDbContext _db;
    public InventoryRepository(WarehouseDbContext db) => _db = db;

    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(int? productId = null, int? warehouseId = null, CancellationToken ct = default)
    {
        var query = _db.Inventory.AsNoTracking().AsQueryable();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        if (warehouseId.HasValue) query = query.Where(x => x.WarehouseId == warehouseId.Value);
        return await query.OrderBy(x => x.WarehouseId).ThenBy(x => x.ProductId).ToListAsync(ct);
    }

    public Task<InventoryItem?> GetByIdAsync(int productId, int warehouseId, CancellationToken ct = default) =>
        _db.Inventory.AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == productId && x.WarehouseId == warehouseId, ct);

    public Task<InventoryItem?> GetTrackedByIdAsync(int productId, int warehouseId, CancellationToken ct = default) =>
        _db.Inventory.FirstOrDefaultAsync(x => x.ProductId == productId && x.WarehouseId == warehouseId, ct);

    public Task<bool> ProductExistsAsync(int productId, CancellationToken ct = default) =>
        _db.Products.AnyAsync(p => p.ProductId == productId, ct);

    public Task<bool> WarehouseExistsAsync(int warehouseId, CancellationToken ct = default) =>
        _db.Warehouses.AnyAsync(w => w.WarehouseId == warehouseId, ct);

    public Task<bool> ExistsAsync(int productId, int warehouseId, CancellationToken ct = default) =>
        _db.Inventory.AnyAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId, ct);

    public async Task AddAsync(InventoryItem item, CancellationToken ct = default) =>
        await _db.Inventory.AddAsync(item, ct);

    public Task DeleteAsync(InventoryItem item, CancellationToken ct = default)
    {
        _db.Inventory.Remove(item);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

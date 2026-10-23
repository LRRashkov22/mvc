using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;

namespace WebApplication1.Repositories.Implementations;

public sealed class WarehouseRepository : IWarehouseRepository
{
    private readonly WarehouseDbContext _db;
    public WarehouseRepository(WarehouseDbContext db) => _db = db;

    public async Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Warehouses.AsNoTracking().OrderBy(w => w.Name).ToListAsync(ct);

    public Task<Warehouse?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.WarehouseId == id, ct);

    public Task<Warehouse?> GetTrackedByIdAsync(int id, CancellationToken ct = default) =>
        _db.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == id, ct);

    public async Task<bool> HasInventoryOrDeliveriesAsync(int id, CancellationToken ct = default) =>
        await _db.Inventory.AnyAsync(i => i.WarehouseId == id, ct)
        || await _db.Deliveries.AnyAsync(d => d.WarehouseId == id, ct);

    public async Task AddAsync(Warehouse warehouse, CancellationToken ct = default) =>
        await _db.Warehouses.AddAsync(warehouse, ct);

    public Task DeleteAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        _db.Warehouses.Remove(warehouse);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

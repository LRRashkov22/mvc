using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;

namespace WebApplication1.Repositories.Implementations;

public sealed class DeliveryRepository : IDeliveryRepository
{
    private readonly WarehouseDbContext _db;
    public DeliveryRepository(WarehouseDbContext db) => _db = db;

    public async Task<IReadOnlyList<Delivery>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Deliveries.AsNoTracking().Include(d => d.Product).Include(d => d.Warehouse)
            .OrderByDescending(d => d.DeliveryDate).ToListAsync(ct);

    public Task<Delivery?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Deliveries.AsNoTracking().Include(d => d.Product).Include(d => d.Warehouse)
            .FirstOrDefaultAsync(d => d.DeliveryId == id, ct);

    public Task<Delivery?> GetTrackedByIdAsync(int id, CancellationToken ct = default) =>
        _db.Deliveries.FirstOrDefaultAsync(d => d.DeliveryId == id, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        _db.Deliveries.AnyAsync(d => d.DeliveryId == id, ct);

    public async Task AddAsync(Delivery delivery, CancellationToken ct = default) =>
        await _db.Deliveries.AddAsync(delivery, ct);

    public Task DeleteAsync(Delivery delivery, CancellationToken ct = default)
    {
        _db.Deliveries.Remove(delivery);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct = default) =>
        await _db.Products.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Warehouse>> GetWarehousesAsync(CancellationToken ct = default) =>
        await _db.Warehouses.AsNoTracking().OrderBy(w => w.Name).ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

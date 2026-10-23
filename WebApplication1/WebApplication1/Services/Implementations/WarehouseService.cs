
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Services.Implementations;

public class WarehouseService : IWarehouseService
{
    private readonly WarehouseDbContext _context;

    public WarehouseService(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .ToListAsync(ct);
    }

    public async Task<Warehouse?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WarehouseId == id, ct);
    }

    public async Task<Warehouse> CreateAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        // Let the database generate the key and ignore any related data sent by the client
        warehouse.WarehouseId = 0;
        warehouse.Name = warehouse.Name.Trim();
        warehouse.InventoryItems = new List<InventoryItem>();
        warehouse.Deliveries = new List<Delivery>();

        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync(ct);

        return warehouse;
    }

    public async Task<bool> UpdateAsync(int id, Warehouse warehouse, CancellationToken ct = default)
    {
        var existing = await _context.Warehouses.FindAsync(new object[] { id }, ct);
        if (existing is null)
            return false;

        // Only copy the scalar properties, so navigation collections are never overwritten
        existing.Name = warehouse.Name.Trim();

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<DeleteWarehouseResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var info = await _context.Warehouses
            .Where(w => w.WarehouseId == id)
            .Select(w => new
            {
                Warehouse = w,
                HasItems = w.InventoryItems.Any(),
                HasDeliveries = w.Deliveries.Any()
            })
            .FirstOrDefaultAsync(ct);

        if (info is null)
            return DeleteWarehouseResult.NotFound;

        if (info.HasItems || info.HasDeliveries)
            return DeleteWarehouseResult.HasRelatedData;

        _context.Warehouses.Remove(info.Warehouse);
        await _context.SaveChangesAsync(ct);
        return DeleteWarehouseResult.Deleted;
    }
}
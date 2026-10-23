using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Services.Implementations;

public sealed class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouses;
    public WarehouseService(IWarehouseRepository warehouses) => _warehouses = warehouses;

    public Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken ct = default) =>
        GetAllInternalAsync(ct);

    private async Task<IEnumerable<Warehouse>> GetAllInternalAsync(CancellationToken ct) =>
        await _warehouses.GetAllAsync(ct);

    public Task<Warehouse?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _warehouses.GetByIdAsync(id, ct);

    public async Task<Warehouse> CreateAsync(Warehouse warehouse, CancellationToken ct = default)
    {
        warehouse.WarehouseId = 0;
        warehouse.Name = warehouse.Name.Trim();
        warehouse.InventoryItems = new List<InventoryItem>();
        warehouse.Deliveries = new List<Delivery>();
        await _warehouses.AddAsync(warehouse, ct);
        await _warehouses.SaveChangesAsync(ct);
        return warehouse;
    }

    public async Task<bool> UpdateAsync(int id, Warehouse warehouse, CancellationToken ct = default)
    {
        var existing = await _warehouses.GetTrackedByIdAsync(id, ct);
        if (existing is null) return false;
        existing.Name = warehouse.Name.Trim();
        await _warehouses.SaveChangesAsync(ct);
        return true;
    }

    public async Task<DeleteWarehouseResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var warehouse = await _warehouses.GetTrackedByIdAsync(id, ct);
        if (warehouse is null) return DeleteWarehouseResult.NotFound;
        if (await _warehouses.HasInventoryOrDeliveriesAsync(id, ct))
            return DeleteWarehouseResult.HasRelatedData;
        await _warehouses.DeleteAsync(warehouse, ct);
        await _warehouses.SaveChangesAsync(ct);
        return DeleteWarehouseResult.Deleted;
    }
}

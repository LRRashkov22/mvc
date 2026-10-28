using WebApplication1.Models;

namespace WebApplication1.Repositories.Contracts;

public interface IWarehouseRepository
{
    Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken ct = default);
    Task<Warehouse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Warehouse?> GetTrackedByIdAsync(int id, CancellationToken ct = default);
    Task<bool> HasInventoryOrDeliveriesAsync(int id, CancellationToken ct = default);
    Task AddAsync(Warehouse warehouse, CancellationToken ct = default);
    Task DeleteAsync(Warehouse warehouse, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

namespace WebApplication1.Services.Contracts;

using WebApplication1.Models;

public enum DeleteWarehouseResult
{
    Deleted,
    NotFound,
    HasRelatedData
}

public interface IWarehouseService
{
    Task<IEnumerable<Warehouse>> GetAllAsync(CancellationToken ct = default);
    Task<Warehouse?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Warehouse> CreateAsync(Warehouse warehouse, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, Warehouse warehouse, CancellationToken ct = default);
    Task<DeleteWarehouseResult> DeleteAsync(int id, CancellationToken ct = default);
}
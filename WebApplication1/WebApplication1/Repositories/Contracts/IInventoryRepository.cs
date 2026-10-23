using WebApplication1.Models;

namespace WebApplication1.Repositories.Contracts;

public interface IInventoryRepository
{
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(int? productId = null, int? warehouseId = null, CancellationToken ct = default);
    Task<InventoryItem?> GetByIdAsync(int productId, int warehouseId, CancellationToken ct = default);
    Task<InventoryItem?> GetTrackedByIdAsync(int productId, int warehouseId, CancellationToken ct = default);
    Task<bool> ProductExistsAsync(int productId, CancellationToken ct = default);
    Task<bool> WarehouseExistsAsync(int warehouseId, CancellationToken ct = default);
    Task<bool> ExistsAsync(int productId, int warehouseId, CancellationToken ct = default);
    Task AddAsync(InventoryItem item, CancellationToken ct = default);
    Task DeleteAsync(InventoryItem item, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

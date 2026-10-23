using WebApplication1.Models;

namespace WebApplication1.Repositories.Contracts;

public interface IDeliveryRepository
{
    Task<IReadOnlyList<Delivery>> GetAllAsync(CancellationToken ct = default);
    Task<Delivery?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Delivery?> GetTrackedByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task AddAsync(Delivery delivery, CancellationToken ct = default);
    Task DeleteAsync(Delivery delivery, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Warehouse>> GetWarehousesAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

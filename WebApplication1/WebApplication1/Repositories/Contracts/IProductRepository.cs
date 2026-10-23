using WebApplication1.Models;

namespace WebApplication1.Repositories.Contracts;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(int? categoryId = null, string? search = null, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Product?> GetTrackedByIdAsync(int id, CancellationToken ct = default);
    Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct = default);
    Task<bool> HasInventoryOrDeliveriesAsync(int productId, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Product product, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

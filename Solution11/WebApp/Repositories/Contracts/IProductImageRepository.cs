using WebApplication1.Models;

namespace WebApplication1.Repositories.Contracts;

public interface IProductImageRepository
{
    Task<bool> ProductExistsAsync(int productId, CancellationToken ct = default);
    Task<int> CountByProductAsync(int productId, CancellationToken ct = default);
    Task<ProductImage?> GetAsync(int productId, int imageId, CancellationToken ct = default);
    Task AddAsync(ProductImage image, CancellationToken ct = default);
    Task DeleteAsync(ProductImage image, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
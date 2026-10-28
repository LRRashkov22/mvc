namespace WebApplication1.Services.Contracts;

using WebApplication1.Models;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync(int? categoryId = null, string? search = null, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <exception cref="KeyNotFoundException">Category does not exist.</exception>
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);

    /// <summary>Returns null if the product is not found.</summary>
    /// <exception cref="KeyNotFoundException">Category does not exist.</exception>
    Task<Product?> UpdateAsync(int id, Product product, CancellationToken ct = default);

    /// <summary>Returns false if the product is not found.</summary>
    /// <exception cref="InvalidOperationException">Product still has inventory or deliveries.</exception>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
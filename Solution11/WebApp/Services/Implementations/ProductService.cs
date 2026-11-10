using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Services.Implementations;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _products;
    private readonly IFileStorageService _files;

    public ProductService(IProductRepository products, IFileStorageService files)
    {
        _products = products;
        _files = files;
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(int? categoryId = null, string? search = null, CancellationToken ct = default) =>
        _products.GetAllAsync(categoryId, search, ct);

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _products.GetByIdAsync(id, ct);

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        await EnsureCategoryExistsAsync(product.CategoryId, ct);
        var entity = new Product { Name = product.Name.Trim(), Price = product.Price, CategoryId = product.CategoryId };
        await _products.AddAsync(entity, ct);
        await _products.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Product?> UpdateAsync(int id, Product product, CancellationToken ct = default)
    {
        var entity = await _products.GetTrackedByIdAsync(id, ct);
        if (entity is null) return null;
        if (entity.CategoryId != product.CategoryId) await EnsureCategoryExistsAsync(product.CategoryId, ct);
        entity.Name = product.Name.Trim();
        entity.Price = product.Price;
        entity.CategoryId = product.CategoryId;
        await _products.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _products.GetTrackedByIdAsync(id, ct);
        if (entity is null) return false;
        if (await _products.HasInventoryOrDeliveriesAsync(id, ct))
            throw new InvalidOperationException("Product has inventory or delivery records and cannot be deleted.");
        await _products.DeleteAsync(entity, ct);
        await _products.SaveChangesAsync(ct);

        // Image rows are removed by the cascade delete; remove the files from disk too.
        try { _files.DeleteFolder($"products/{id}"); }
        catch (IOException) { /* orphaned files are harmless */ }
        catch (UnauthorizedAccessException) { /* orphaned files are harmless */ }

        return true;
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken ct)
    {
        if (!await _products.CategoryExistsAsync(categoryId, ct))
            throw new KeyNotFoundException($"Category {categoryId} not found.");
    }
}
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;

namespace WebApplication1.Repositories.Implementations;

public sealed class ProductImageRepository : IProductImageRepository
{
    private readonly WarehouseDbContext _db;
    public ProductImageRepository(WarehouseDbContext db) => _db = db;

    public Task<bool> ProductExistsAsync(int productId, CancellationToken ct = default) =>
        _db.Products.AnyAsync(p => p.ProductId == productId, ct);

    public Task<int> CountByProductAsync(int productId, CancellationToken ct = default) =>
        _db.ProductImages.CountAsync(i => i.ProductId == productId, ct);

    public Task<ProductImage?> GetAsync(int productId, int imageId, CancellationToken ct = default) =>
        _db.ProductImages.FirstOrDefaultAsync(
            i => i.ProductId == productId && i.ProductImageId == imageId, ct);

    public async Task AddAsync(ProductImage image, CancellationToken ct = default) =>
        await _db.ProductImages.AddAsync(image, ct);

    public Task DeleteAsync(ProductImage image, CancellationToken ct = default)
    {
        _db.ProductImages.Remove(image);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
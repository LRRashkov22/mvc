namespace WebApplication1.Services.Implementations;

using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

public class ProductService : IProductService
{
    private readonly WarehouseDbContext _db;

    public ProductService(WarehouseDbContext db) => _db = db;

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        int? categoryId = null, string? search = null, CancellationToken ct = default)
    {
        var query = _db.Products.AsNoTracking().AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Name.Contains(term));
        }

        return await query.OrderBy(p => p.Name).ToListAsync(ct);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == id, ct);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        await EnsureCategoryExistsAsync(product.CategoryId, ct);

        // Only accept the scalar fields from the request body
        var entity = new Product
        {
            Name = product.Name,
            Price = product.Price,
            CategoryId = product.CategoryId
        };

        _db.Products.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Product?> UpdateAsync(int id, Product product, CancellationToken ct = default)
    {
        var entity = await _db.Products.FindAsync(new object[] { id }, ct);
        if (entity is null) return null;

        if (entity.CategoryId != product.CategoryId)
            await EnsureCategoryExistsAsync(product.CategoryId, ct);

        entity.Name = product.Name;
        entity.Price = product.Price;
        entity.CategoryId = product.CategoryId;

        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Products.FindAsync(new object[] { id }, ct);
        if (entity is null) return false;

        // Required FKs cascade by default, so block deletion instead of silently wiping stock/delivery history
        var inUse = await _db.Inventory.AnyAsync(i => i.ProductId == id, ct)
                    || await _db.Deliveries.AnyAsync(d => d.ProductId == id, ct);
        if (inUse)
            throw new InvalidOperationException("Product has inventory or delivery records and cannot be deleted.");

        _db.Products.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken ct)
    {
        if (!await _db.Categories.AnyAsync(c => c.CategoryId == categoryId, ct))
            throw new KeyNotFoundException($"Category {categoryId} not found.");
    }
}
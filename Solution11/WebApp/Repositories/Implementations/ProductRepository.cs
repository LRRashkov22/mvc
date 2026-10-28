using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;

namespace WebApplication1.Repositories.Implementations;

public sealed class ProductRepository : IProductRepository
{
    private readonly WarehouseDbContext _db;
    public ProductRepository(WarehouseDbContext db) => _db = db;

    public async Task<IReadOnlyList<Product>> GetAllAsync(int? categoryId = null, string? search = null, CancellationToken ct = default)
    {
        var query = _db.Products.AsNoTracking().AsQueryable();
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Name.Contains(term));
        }
        return await query.OrderBy(p => p.Name).ToListAsync(ct);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id, ct);

    public Task<Product?> GetTrackedByIdAsync(int id, CancellationToken ct = default) =>
        _db.Products.FirstOrDefaultAsync(p => p.ProductId == id, ct);

    public Task<bool> CategoryExistsAsync(int categoryId, CancellationToken ct = default) =>
        _db.Categories.AnyAsync(c => c.CategoryId == categoryId, ct);

    public async Task<bool> HasInventoryOrDeliveriesAsync(int productId, CancellationToken ct = default) =>
        await _db.Inventory.AnyAsync(i => i.ProductId == productId, ct)
        || await _db.Deliveries.AnyAsync(d => d.ProductId == productId, ct);

    public async Task AddAsync(Product product, CancellationToken ct = default) =>
        await _db.Products.AddAsync(product, ct);

    public Task DeleteAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Remove(product);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

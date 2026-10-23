using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;

namespace WebApplication1.Repositories.Implementations;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly WarehouseDbContext _db;
    public CategoryRepository(WarehouseDbContext db) => _db = db;

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == id, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        _db.Categories.AnyAsync(c => c.CategoryId == id, ct);

    public Task<bool> HasProductsAsync(int id, CancellationToken ct = default) =>
        _db.Products.AnyAsync(p => p.CategoryId == id, ct);

    public async Task AddAsync(Category category, CancellationToken ct = default) =>
        await _db.Categories.AddAsync(category, ct);

    public Task UpdateAsync(Category category, CancellationToken ct = default) =>
        Task.CompletedTask; // The entity is tracked by the service's scoped DbContext.

    public Task DeleteAsync(Category category, CancellationToken ct = default)
    {
        _db.Categories.Remove(category);
        return Task.CompletedTask;
    }

    public async Task<Category?> GetTrackedByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id, ct);
    }
    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

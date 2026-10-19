namespace WebApplication1.Services.Implementations;


using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services.Contracts;

public class CategoriesService : ICategoriesService
{
    private readonly WarehouseDbContext _context;

    public CategoriesService(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }

    public async Task CreateAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        var existingCategory = await _context.Categories
            .FindAsync(category.CategoryId);

        if (existingCategory == null)
            return;

        existingCategory.Name = category.Name;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories
            .FindAsync(id);

        if (category == null)
            return;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}

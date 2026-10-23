using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;
using WebApplication1.Services.Contracts;

namespace WebApplication1.Services.Implementations;

public sealed class CategoriesService : ICategoriesService
{
    private readonly ICategoryRepository _categories;
    public CategoriesService(ICategoryRepository categories) => _categories = categories;

    public async Task<IEnumerable<Category>> GetAllAsync() =>
        await _categories.GetAllAsync();

    public Task<Category?> GetByIdAsync(int id) =>
        _categories.GetByIdAsync(id);

    public async Task CreateAsync(Category category)
    {
        category.Name = category.Name.Trim();
        await _categories.AddAsync(category);
        await _categories.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        var existing = await _categories.GetTrackedByIdAsync(category.CategoryId);
        if (existing is null) return;
        existing.Name = category.Name.Trim();
        await _categories.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _categories.GetTrackedByIdAsync(id);
        if (category is null) return;
        if (await _categories.HasProductsAsync(id))
            throw new InvalidOperationException("Category has products and cannot be deleted.");
        await _categories.DeleteAsync(category);
        await _categories.SaveChangesAsync();
    }
}

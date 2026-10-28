namespace WebApplication1.Services.Contracts;

using WebApplication1.Models;

public interface ICategoriesService
{
    Task<IEnumerable<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task CreateAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(int id);
}
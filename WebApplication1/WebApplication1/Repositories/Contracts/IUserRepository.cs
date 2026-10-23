using WebApplication1.Models;

namespace WebApplication1.Repositories.Contracts;

public interface IUserRepository
{
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

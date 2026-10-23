using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Contracts;

namespace WebApplication1.Repositories.Implementations;

public sealed class UserRepository : IUserRepository
{
    private readonly WarehouseDbContext _db;
    public UserRepository(WarehouseDbContext db) => _db = db;

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default) =>
        _db.User.AnyAsync(u => u.Username == username, ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        _db.User.FirstOrDefaultAsync(u => u.Username == username, ct);

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.User.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await _db.User.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

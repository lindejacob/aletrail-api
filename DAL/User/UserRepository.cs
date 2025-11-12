using aletrail_api.Interfaces;
using aletrail_api.Models;
using Microsoft.EntityFrameworkCore;

namespace aletrail_api.DAL;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<int> CreateAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user.Id;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _db.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}
using aletrail_api.Models;
using Microsoft.EntityFrameworkCore;

namespace aletrail_api.DAL.Account;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IEnumerable<User>> getAllUsersAsync()
    {
        return await _dbContext.Users.OrderBy(u => u.Username).ToListAsync();
    }

    public async Task<User?> getUserByIdAsync(int id)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> getUserByEmailAsync(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbContext.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<int> insertUserAsync(User user)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user.Id;
    }

    public async Task<int> updateUserAsync(User user)
    {
        _dbContext.Users.Update(user);
        return await _dbContext.SaveChangesAsync();
    }

    public async Task<int> deleteUserAsync(int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return 0;
        
        _dbContext.Users.Remove(user);
        return await _dbContext.SaveChangesAsync();
    }
}
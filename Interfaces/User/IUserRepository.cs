using aletrail_api.Models;

namespace aletrail_api.Interfaces;

public interface IUserRepository
{
    Task<int> CreateAsync(User user);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
}
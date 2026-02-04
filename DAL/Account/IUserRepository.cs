using aletrail_api.Dtos.User;
using aletrail_api.Models;

namespace aletrail_api.DAL.Account;

public interface IUserRepository
{
    Task<IEnumerable<User>> getAllUsersAsync();
    Task<User?> getUserByIdAsync(int id);
    Task<User?> getUserByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    Task<int> insertUserAsync(User user);
    Task<int> updateUserAsync(User user);
    Task<int> deleteUserAsync(int id);
}
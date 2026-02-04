using aletrail_api.Models;

namespace aletrail_api.DAL.Account;

public class UserRepository : IUserRepository
{
    public Task<IEnumerable<User>> getAllUsersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<User?> getUserByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<User?> getUserByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UsernameExistsAsync(string username)
    {
        throw new NotImplementedException();
    }

    public Task<int> insertUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task<int> updateUserAsync(User user)
    {
        throw new NotImplementedException();
    }

    public Task<int> deleteUserAsync(int id)
    {
        throw new NotImplementedException();
    }
}
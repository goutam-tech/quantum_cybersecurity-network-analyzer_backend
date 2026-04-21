using network_project.Models;

namespace network_project.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}

public interface ITokenRepository
{
    Task SaveTokenAsync(UserToken token);
    Task<UserToken?> GetTokenAsync(string token);
    Task RevokeTokenAsync(string token);
    Task<bool> IsTokenValidAsync(string token);
    Task SaveChangesAsync();
}
using Microsoft.EntityFrameworkCore;
using network_project.Data;
using network_project.Interfaces;
using network_project.Models;

namespace network_project.Repository;
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByEmailAsync(string email)
        => await _db.Users
                    .Include(u => u.Tokens)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<User?> GetByIdAsync(int id)
        => await _db.Users
                    .Include(u => u.Tokens)
                    .FirstOrDefaultAsync(u => u.Id == id);

    public async Task AddAsync(User user)
        => await _db.Users.AddAsync(user);

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}

public class TokenRepository : ITokenRepository
{
    private readonly AppDbContext _db;
    public TokenRepository(AppDbContext db) => _db = db;

    public async Task SaveTokenAsync(UserToken token)
    {
        await _db.UserTokens.AddAsync(token);
        await _db.SaveChangesAsync();
    }

    public async Task<UserToken?> GetTokenAsync(string token)
        => await _db.UserTokens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Token == token);

    public async Task RevokeTokenAsync(string token)
    {
        var userToken = await GetTokenAsync(token);
        if (userToken is not null)
        {
            userToken.IsRevoked = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> IsTokenValidAsync(string token)
    {
        var userToken = await GetTokenAsync(token);
        return userToken is not null
            && !userToken.IsRevoked
            && userToken.ExpiresAt > DateTime.UtcNow;
    }

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}
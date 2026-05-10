using Microsoft.EntityFrameworkCore;
using Moq;
using network_project.Data;
using network_project.Interfaces;
using network_project.Models;
using network_project.Repository;

namespace network_project.Tests.Interfaces;

public class UserRepositoryInterfaceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task IUserRepository_GetByEmailAsync_Should_Return_User()
    {
        var context = GetDbContext();

        var user = new User
        {
            Email = "test@example.com"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        IUserRepository repository = new UserRepository(context);

        var result = await repository.GetByEmailAsync("test@example.com");

        Assert.NotNull(result);
        Assert.Equal(user.Email, result!.Email);
    }

    [Fact]
    public async Task IUserRepository_GetByIdAsync_Should_Return_User()
    {
        var context = GetDbContext();

        var user = new User
        {
            Email = "user@example.com"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        IUserRepository repository = new UserRepository(context);

        var result = await repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result!.Id);
    }

    [Fact]
    public async Task IUserRepository_AddAsync_Should_Add_User()
    {
        var context = GetDbContext();

        IUserRepository repository = new UserRepository(context);

        var user = new User
        {
            Email = "new@example.com"
        };

        await repository.AddAsync(user);
        await repository.SaveChangesAsync();

        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task ITokenRepository_SaveTokenAsync_Should_Save_Token()
    {
        var context = GetDbContext();

        var user = new User
        {
            Email = "token@example.com"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        ITokenRepository repository = new TokenRepository(context);

        var token = new UserToken
        {
            UserId = user.Id,
            Token = "abc123",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsRevoked = false
        };

        await repository.SaveTokenAsync(token);

        Assert.Equal(1, await context.UserTokens.CountAsync());
    }

    [Fact]
    public async Task ITokenRepository_GetTokenAsync_Should_Return_Token()
    {
        var context = GetDbContext();

        var user = new User
        {
            Email = "test@example.com"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = new UserToken
        {
            UserId = user.Id,
            Token = "token123",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsRevoked = false
        };

        context.UserTokens.Add(token);
        await context.SaveChangesAsync();

        ITokenRepository repository = new TokenRepository(context);

        var result = await repository.GetTokenAsync("token123");

        Assert.NotNull(result);
        Assert.Equal("token123", result!.Token);
    }

    [Fact]
    public async Task ITokenRepository_RevokeTokenAsync_Should_Revoke_Token()
    {
        var context = GetDbContext();

        var user = new User
        {
            Email = "revoke@example.com"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = new UserToken
        {
            UserId = user.Id,
            Token = "revoke-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsRevoked = false
        };

        context.UserTokens.Add(token);
        await context.SaveChangesAsync();

        ITokenRepository repository = new TokenRepository(context);

        await repository.RevokeTokenAsync("revoke-token");

        var updated = await context.UserTokens
            .FirstOrDefaultAsync(t => t.Token == "revoke-token");

        Assert.NotNull(updated);
        Assert.True(updated!.IsRevoked);
    }

    [Fact]
    public async Task ITokenRepository_IsTokenValidAsync_Should_Return_True()
    {
        var context = GetDbContext();

        var user = new User
        {
            Email = "valid@example.com"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var token = new UserToken
        {
            UserId = user.Id,
            Token = "valid-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsRevoked = false
        };

        context.UserTokens.Add(token);
        await context.SaveChangesAsync();

        ITokenRepository repository = new TokenRepository(context);

        var result = await repository.IsTokenValidAsync("valid-token");

        Assert.True(result);
    }
}
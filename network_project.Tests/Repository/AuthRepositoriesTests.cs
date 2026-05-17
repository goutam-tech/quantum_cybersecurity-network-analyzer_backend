using Microsoft.EntityFrameworkCore;
using network_project.Data;
using network_project.Models;
using network_project.Repository;

namespace network_project.Tests.Repository;

public class UserRepositoryTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetByEmailAsync_Should_Return_User_When_Email_Exists()
    {
        var context = GetDbContext();

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Tokens = new List<UserToken>()
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var result = await repository.GetByEmailAsync("TEST@example.com");

        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_Should_Return_Null_When_Email_Not_Exists()
    {
        var context = GetDbContext();
        var repository = new UserRepository(context);

        var result = await repository.GetByEmailAsync("notfound@example.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_User_When_Id_Exists()
    {
        var context = GetDbContext();

        var user = new User
        {
            Id = 1,
            Email = "user@example.com",
            Tokens = new List<UserToken>()
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var result = await repository.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task AddAsync_Should_Add_User()
    {
        var context = GetDbContext();
        var repository = new UserRepository(context);

        var user = new User
        {
            Email = "newuser@example.com"
        };

        await repository.AddAsync(user);
        await repository.SaveChangesAsync();

        Assert.Equal(1, await context.Users.CountAsync());
    }
}

public class TokenRepositoryTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task SaveTokenAsync_Should_Save_Token()
    {
        // Arrange
        var context = GetDbContext();
        var repository = new TokenRepository(context);

        var token = new UserToken
        {
            Token = "abc123",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsRevoked = false
        };

        await repository.SaveTokenAsync(token);

        Assert.Equal(1, await context.UserTokens.CountAsync());
    }

    [Fact]
    public async Task GetTokenAsync_Should_Return_Token_When_Exists()
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
            Token = "valid-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsRevoked = false
        };

        context.UserTokens.Add(token);
        await context.SaveChangesAsync();

        var repository = new TokenRepository(context);

        var result = await repository.GetTokenAsync("valid-token");

        Assert.NotNull(result);
        Assert.Equal("valid-token", result!.Token);
    }

    [Fact]
    public async Task RevokeTokenAsync_Should_Revoke_Token()
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
            Token = "revoke-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsRevoked = false
        };

        context.UserTokens.Add(token);
        await context.SaveChangesAsync();

        var repository = new TokenRepository(context);

        await repository.RevokeTokenAsync("revoke-token");

        var updatedToken = await context.UserTokens
            .FirstOrDefaultAsync(t => t.Token == "revoke-token");

        Assert.NotNull(updatedToken);
        Assert.True(updatedToken!.IsRevoked);
    }

    [Fact]
    public async Task IsTokenValidAsync_Should_Return_True_When_Token_Is_Valid()
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
            Token = "valid-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsRevoked = false
        };

        context.UserTokens.Add(token);
        await context.SaveChangesAsync();

        var repository = new TokenRepository(context);

        var result = await repository.IsTokenValidAsync("valid-token");

        Assert.True(result);
    }

    [Fact]
    public async Task IsTokenValidAsync_Should_Return_False_When_Token_Is_Revoked()
    {
        var context = GetDbContext();

        var token = new UserToken
        {
            Token = "revoked-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsRevoked = true
        };

        context.UserTokens.Add(token);
        await context.SaveChangesAsync();

        var repository = new TokenRepository(context);

        var result = await repository.IsTokenValidAsync("revoked-token");

        Assert.False(result);
    }

    [Fact]
    public async Task IsTokenValidAsync_Should_Return_False_When_Token_Is_Expired()
    {
        var context = GetDbContext();

        var token = new UserToken
        {
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            IsRevoked = false
        };

        context.UserTokens.Add(token);
        await context.SaveChangesAsync();

        var repository = new TokenRepository(context);

        var result = await repository.IsTokenValidAsync("expired-token");

        Assert.False(result);
    }
}
using System.ComponentModel.DataAnnotations;
using network_project.Models;

namespace network_project.Tests.Models;

public class UserAndTokenModelTests
{
    [Fact]
    public void User_Should_Create_Instance_With_Default_Values()
    {
        var user = new User();

        Assert.Equal(string.Empty, user.Email);
        Assert.Equal(string.Empty, user.Name);
        Assert.Equal(string.Empty, user.PasswordHash);
        Assert.NotNull(user.Tokens);
    }

    [Fact]
    public void UserToken_Should_Create_Instance_With_Default_Values()
    {
        var token = new UserToken();

        Assert.Equal(string.Empty, token.Token);
        Assert.False(token.IsRevoked);
    }

    [Fact]
    public void User_Should_Set_Properties()
    {
        var createdAt = DateTime.UtcNow;

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Name = "Test",
            PasswordHash = "hash",
            CreatedAt = createdAt
        };

        Assert.Equal(1, user.Id);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("Test", user.Name);
        Assert.Equal("hash", user.PasswordHash);
        Assert.Equal(createdAt, user.CreatedAt);
    }

    [Fact]
    public void UserToken_Should_Set_Properties()
    {
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            PasswordHash = "hash"
        };

        var expiresAt = DateTime.UtcNow.AddHours(1);

        var token = new UserToken
        {
            Id = 1,
            UserId = 1,
            Token = "jwt-token",
            ExpiresAt = expiresAt,
            IsRevoked = true,
            User = user
        };

        Assert.Equal(1, token.Id);
        Assert.Equal(1, token.UserId);
        Assert.Equal("jwt-token", token.Token);
        Assert.Equal(expiresAt, token.ExpiresAt);
        Assert.True(token.IsRevoked);
        Assert.Equal(user, token.User);
    }

    [Fact]
    public void User_Email_Should_Have_Required_Attribute()
    {
        var property = typeof(User).GetProperty(nameof(User.Email));

        var attribute = Attribute.GetCustomAttribute(
            property!,
            typeof(RequiredAttribute));

        Assert.NotNull(attribute);
    }

    [Fact]
    public void User_Email_Should_Have_MaxLength_100()
    {
        var property = typeof(User).GetProperty(nameof(User.Email));

        var attribute = (MaxLengthAttribute?)Attribute.GetCustomAttribute(
            property!,
            typeof(MaxLengthAttribute));

        Assert.NotNull(attribute);
        Assert.Equal(100, attribute!.Length);
    }

    [Fact]
    public void User_Name_Should_Have_MaxLength_100()
    {
        var property = typeof(User).GetProperty(nameof(User.Name));

        var attribute = (MaxLengthAttribute?)Attribute.GetCustomAttribute(
            property!,
            typeof(MaxLengthAttribute));

        Assert.NotNull(attribute);
        Assert.Equal(100, attribute!.Length);
    }

    [Fact]
    public void User_PasswordHash_Should_Have_Required_Attribute()
    {
        var property = typeof(User).GetProperty(nameof(User.PasswordHash));

        var attribute = Attribute.GetCustomAttribute(
            property!,
            typeof(RequiredAttribute));

        Assert.NotNull(attribute);
    }

    [Fact]
    public void UserToken_Token_Should_Have_Required_Attribute()
    {
        var property = typeof(UserToken).GetProperty(nameof(UserToken.Token));

        var attribute = Attribute.GetCustomAttribute(
            property!,
            typeof(RequiredAttribute));

        Assert.NotNull(attribute);
    }
}
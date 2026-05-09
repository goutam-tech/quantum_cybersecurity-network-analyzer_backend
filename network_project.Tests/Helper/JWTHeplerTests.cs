using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using network_project.Helper;
using network_project.Models;
using System.Security.Claims;
using Xunit;

namespace network_project.Tests.Helper
{
    public class JwtHelperTests
    {
        private readonly JwtHelper _jwtHelper;

        public JwtHelperTests()
        {
            var settings = new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "THIS_IS_A_VERY_SECURE_SECRET_KEY_12345" },
                { "JwtSettings:Issuer", "QuantumCyberAnalyzer" },
                { "JwtSettings:Audience", "QuantumCyberAnalyzerUsers" },
                { "JwtSettings:ExpiryHours", "1" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            _jwtHelper = new JwtHelper(configuration);
        }

        [Fact]
        public void GenerateToken_Should_Return_Valid_Token()
        {
            var user = new User
            {
                Id = 1,
                Email = "test@mail.com",
                Name = "Test User"
            };

            var result = _jwtHelper.GenerateToken(user);

            Assert.NotNull(result.Token);
            Assert.NotEmpty(result.Token);

            Assert.True(result.ExpiresAt > DateTime.UtcNow);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token);

            Assert.Equal("QuantumCyberAnalyzer", jwt.Issuer);
        }

        [Fact]
        public void ValidateToken_Should_Return_Principal_When_Token_Is_Valid()
        {
            var user = new User
            {
                Id = 5,
                Email = "valid@mail.com",
                Name = "Valid User"
            };

            var token = _jwtHelper.GenerateToken(user).Token;

            var principal = _jwtHelper.ValidateToken(token);

            Assert.NotNull(principal);

            var emailClaim =
                principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)
                ?? principal?.FindFirst(JwtRegisteredClaimNames.Email);

            Assert.NotNull(emailClaim);

            Assert.Equal(
                "valid@mail.com",
                emailClaim!.Value
            );
        }

        [Fact]
        public void ValidateToken_Should_Return_Null_When_Token_Is_Invalid()
        {
            var invalidToken = "THIS_IS_INVALID_TOKEN";

            var result = _jwtHelper.ValidateToken(invalidToken);

            Assert.Null(result);
        }

        [Fact]
        public void GetUserIdFromToken_Should_Return_UserId()
        {
            var user = new User
            {
                Id = 99,
                Email = "userid@mail.com",
                Name = "User Id Test"
            };

            var token = _jwtHelper.GenerateToken(user).Token;

            var principal = _jwtHelper.ValidateToken(token);

            Assert.NotNull(principal);

            var userIdClaim =
                principal!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);

            Assert.NotNull(userIdClaim);

            var parsed = int.TryParse(userIdClaim!.Value, out var userId);

            Assert.True(parsed);
            Assert.Equal(99, userId);
        }

        [Fact]
        public void GetUserIdFromToken_Should_Return_Null_When_Token_Is_Invalid()
        {
            var invalidToken = "INVALID_TOKEN";

            var result = _jwtHelper.GetUserIdFromToken(invalidToken);

            Assert.Null(result);
        }

        [Fact]
        public void ValidateToken_Should_Return_Null_When_Token_Is_Expired()
        {
            var expiredSettings = new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "THIS_IS_A_VERY_SECURE_SECRET_KEY_12345" },
                { "JwtSettings:Issuer", "QuantumCyberAnalyzer" },
                { "JwtSettings:Audience", "QuantumCyberAnalyzerUsers" },
                { "JwtSettings:ExpiryHours", "-1" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(expiredSettings)
                .Build();

            var expiredJwtHelper = new JwtHelper(configuration);

            var user = new User
            {
                Id = 10,
                Email = "expired@mail.com",
                Name = "Expired User"
            };

            var token = expiredJwtHelper.GenerateToken(user).Token;

            var principal = expiredJwtHelper.ValidateToken(token);

            Assert.Null(principal);
        }

        [Fact]
        public void Constructor_Should_Throw_When_SecretKey_Is_Missing()
        {
            var settings = new Dictionary<string, string?>();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            Assert.Throws<InvalidOperationException>(() =>
            {
                var helper = new JwtHelper(configuration);
            });
        }
    }
}
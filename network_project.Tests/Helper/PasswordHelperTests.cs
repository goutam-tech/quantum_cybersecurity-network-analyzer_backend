using System;
using network_project.Helper;
using Xunit;

namespace network_project.Tests.Helper
{
    public class PasswordHelperTests
    {
        [Fact]
        public void HashPassword_Should_Return_Hash()
        {
            var password = "MySecurePassword123";

            var hash = PasswordHelper.HashPassword(password);

            Assert.NotNull(hash);
            Assert.NotEmpty(hash);

            Assert.StartsWith("$2", hash);
        }

        [Fact]
        public void HashPassword_Should_Return_Different_Hashes_For_Same_Password()
        {
            var password = "SamePassword";

            var hash1 = PasswordHelper.HashPassword(password);
            var hash2 = PasswordHelper.HashPassword(password);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void HashPassword_Should_Throw_When_Password_Is_Empty()
        {
            var password = "";

            var ex = Assert.Throws<ArgumentException>(() =>
            {
                PasswordHelper.HashPassword(password);
            });

            Assert.Equal("password", ex.ParamName);
        }

        [Fact]
        public void HashPassword_Should_Throw_When_Password_Is_Null()
        {
            string? password = null;

            Assert.Throws<ArgumentException>(() =>
            {
                PasswordHelper.HashPassword(password!);
            });
        }

        [Fact]
        public void HashPassword_Should_Throw_When_Password_Is_Whitespace()
        {
            var password = "   ";

            Assert.Throws<ArgumentException>(() =>
            {
                PasswordHelper.HashPassword(password);
            });
        }

        [Fact]
        public void VerifyPassword_Should_Return_True_For_Correct_Password()
        {
            var password = "CorrectPassword123";
            var hash = PasswordHelper.HashPassword(password);

            var result = PasswordHelper.VerifyPassword(password, hash);

            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_Should_Return_False_For_Wrong_Password()
        {
            var hash = PasswordHelper.HashPassword("OriginalPassword");

            var result = PasswordHelper.VerifyPassword(
                "WrongPassword",
                hash
            );

            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_Should_Return_False_When_Password_Is_Empty()
        {
            var hash = PasswordHelper.HashPassword("Password123");

            var result = PasswordHelper.VerifyPassword("", hash);

            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_Should_Return_False_When_Hash_Is_Empty()
        {
            var password = "Password123";

            var result = PasswordHelper.VerifyPassword(password, "");

            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_Should_Return_False_When_Inputs_Are_Null()
        {
            var result = PasswordHelper.VerifyPassword(null!, null!);

            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_Should_Throw_For_Invalid_Hash()
        {
            var password = "Password123";
            var invalidHash = "INVALID_HASH";

            Assert.Throws<BCrypt.Net.SaltParseException>(() =>
            {
                PasswordHelper.VerifyPassword(password, invalidHash);
            });
        }
    }
}
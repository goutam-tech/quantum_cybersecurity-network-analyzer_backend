using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using network_project.Dto;
using Xunit;

namespace network_project.Tests.Dto
{
    public class AuthDtoTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();

            var context = new ValidationContext(model, null, null);

            Validator.TryValidateObject(
                model,
                context,
                results,
                true
            );

            return results;
        }

        [Fact]
        public void SignupDto_Should_Pass_When_Data_Is_Valid()
        {
            var dto = new SignupDto
            {
                Email = "test@example.com",
                Name = "John",
                Password = "password123"
            };

            var results = ValidateModel(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void SignupDto_Should_Fail_When_Email_Is_Invalid()
        {
            var dto = new SignupDto
            {
                Email = "invalid-email",
                Name = "John",
                Password = "password123"
            };

            var results = ValidateModel(dto);

            Assert.Contains(results, r =>
                r.ErrorMessage!.Contains("Email"));
        }

        [Fact]
        public void SignupDto_Should_Fail_When_Name_Is_Too_Short()
        {
            var dto = new SignupDto
            {
                Email = "test@example.com",
                Name = "A",
                Password = "password123"
            };

            var results = ValidateModel(dto);

            Assert.Contains(results, r =>
                r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void SignupDto_Should_Fail_When_Password_Is_Too_Short()
        {
            var dto = new SignupDto
            {
                Email = "test@example.com",
                Name = "John",
                Password = "123"
            };

            var results = ValidateModel(dto);

            Assert.Contains(results, r =>
                r.ErrorMessage == "Password must be at least 6 characters.");
        }

        [Fact]
        public void SignupDto_Should_Fail_When_Fields_Are_Empty()
        {
            var dto = new SignupDto();

            var results = ValidateModel(dto);

            Assert.Equal(3, results.Count);
        }


        [Fact]
        public void LoginDto_Should_Pass_When_Data_Is_Valid()
        {
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var results = ValidateModel(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void LoginDto_Should_Fail_When_Email_Is_Invalid()
        {
            var dto = new LoginDto
            {
                Email = "wrong-email",
                Password = "password123"
            };

            var results = ValidateModel(dto);

            Assert.NotEmpty(results);
        }

        [Fact]
        public void LoginDto_Should_Fail_When_Password_Is_Missing()
        {
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = ""
            };

            var results = ValidateModel(dto);

            Assert.Contains(results, r =>
                r.MemberNames.Contains("Password"));
        }


        [Fact]
        public void RevokeDto_Should_Pass_When_Token_Is_Provided()
        {
            var dto = new RevokeDto
            {
                Token = "sample-token"
            };

            var results = ValidateModel(dto);

            Assert.Empty(results);
        }

        [Fact]
        public void RevokeDto_Should_Fail_When_Token_Is_Empty()
        {
            var dto = new RevokeDto
            {
                Token = ""
            };

            var results = ValidateModel(dto);

            Assert.NotEmpty(results);
        }

        [Fact]
        public void UserDto_Should_Create_Correctly()
        {
            var createdAt = DateTime.UtcNow;

            var dto = new UserDto(
                1,
                "test@example.com",
                "John",
                createdAt
            );

            Assert.Equal(1, dto.Id);
            Assert.Equal("test@example.com", dto.Email);
            Assert.Equal("John", dto.Name);
            Assert.Equal(createdAt, dto.CreatedAt);
        }

        [Fact]
        public void AuthResponseDto_Should_Create_Correctly()
        {
            var user = new UserDto(
                1,
                "test@example.com",
                "John",
                DateTime.UtcNow
            );

            var expiresAt = DateTime.UtcNow.AddHours(1);

            var dto = new AuthResponseDto(
                "jwt-token",
                expiresAt,
                user
            );

            Assert.Equal("jwt-token", dto.Token);
            Assert.Equal(expiresAt, dto.ExpiresAt);
            Assert.Equal(user, dto.User);
        }
    }
}
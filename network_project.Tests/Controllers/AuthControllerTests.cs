using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using network_project.Controllers;
using network_project.Interfaces;
using network_project.Helper;
using network_project.Dto;
using network_project.Models;

using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace network_project.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<ITokenRepository> _tokenRepo = new();
        private readonly JwtHelper _jwt;

        public AuthControllerTests()
        {
            var settings = new Dictionary<string, string>
            {
                { "JwtSettings:SecretKey",   "oICqjDPkwAJr-GXDIjZizyOSXf01iR7X" },
                { "JwtSettings:Issuer",      "test" },
                { "JwtSettings:Audience",    "test" },
                { "JwtSettings:ExpiryHours", "1" }
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();

            _jwt = new JwtHelper(config);
        }

        private AuthController GetController()
        {
            return new AuthController(
                _userRepo.Object,
                _tokenRepo.Object,
                _jwt
            );
        }

        [Fact]
        public async Task Signup_InvalidModel_ReturnsBadRequest()
        {
            var controller = GetController();
            controller.ModelState.AddModelError("Email", "Required");

            var result = await controller.Signup(new SignupDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Signup_UserExists_ReturnsConflict()
        {
            _userRepo.Setup(x => x.GetByEmailAsync("test@mail.com"))
                     .ReturnsAsync(new User());

            var controller = GetController();

            var result = await controller.Signup(new SignupDto
            {
                Email = "test@mail.com",
                Name = "Test",
                Password = "123"
            });

            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Signup_Valid_ReturnsOk()
        {
            _userRepo.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);

            _userRepo.Setup(x => x.AddAsync(It.IsAny<User>()))
                     .Returns(Task.CompletedTask);

            _userRepo.Setup(x => x.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

            _tokenRepo.Setup(x => x.SaveTokenAsync(It.IsAny<UserToken>()))
                      .Returns(Task.CompletedTask);

            var controller = GetController();

            var result = await controller.Signup(new SignupDto
            {
                Email = "test@mail.com",
                Name = "Test",
                Password = "Password123"
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task Login_InvalidModel_ReturnsBadRequest()
        {
            var controller = GetController();
            controller.ModelState.AddModelError("Email", "Required");

            var result = await controller.Login(new LoginDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsUnauthorized()
        {
            _userRepo.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);

            var controller = GetController();

            var result = await controller.Login(new LoginDto
            {
                Email = "test@mail.com",
                Password = "123"
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            var user = new User
            {
                Email = "test@mail.com",
                PasswordHash = PasswordHelper.HashPassword("correct")
            };

            _userRepo.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync(user);

            var controller = GetController();

            var result = await controller.Login(new LoginDto
            {
                Email = "test@mail.com",
                Password = "wrong"
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_Valid_ReturnsOk()
        {
            var user = new User
            {
                Id = 1,
                Email = "test@mail.com",
                Name = "Test",
                PasswordHash = PasswordHelper.HashPassword("correct")
            };

            _userRepo.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync(user);

            _tokenRepo.Setup(x => x.SaveTokenAsync(It.IsAny<UserToken>()))
                      .Returns(Task.CompletedTask);

            var controller = GetController();

            var result = await controller.Login(new LoginDto
            {
                Email = "test@mail.com",
                Password = "correct"
            });

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task Revoke_InvalidModel_ReturnsBadRequest()
        {
            var controller = GetController();
            controller.ModelState.AddModelError("Token", "Required");

            var result = await controller.Revoke(new RevokeDto());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Revoke_TokenNotFound_ReturnsNotFound()
        {
            _tokenRepo.Setup(x => x.GetTokenAsync(It.IsAny<string>()))
                      .ReturnsAsync((UserToken?)null);

            var controller = GetController();

            var result = await controller.Revoke(new RevokeDto { Token = "abc" });

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Revoke_AlreadyRevoked_ReturnsBadRequest()
        {
            _tokenRepo.Setup(x => x.GetTokenAsync(It.IsAny<string>()))
                      .ReturnsAsync(new UserToken { IsRevoked = true });

            var controller = GetController();

            var result = await controller.Revoke(new RevokeDto { Token = "abc" });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Revoke_Valid_ReturnsOk()
        {
            _tokenRepo.Setup(x => x.GetTokenAsync(It.IsAny<string>()))
                      .ReturnsAsync(new UserToken { IsRevoked = false });

            _tokenRepo.Setup(x => x.RevokeTokenAsync(It.IsAny<string>()))
                      .Returns(Task.CompletedTask);

            var controller = GetController();

            var result = await controller.Revoke(new RevokeDto { Token = "abc" });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Me_InvalidToken_ReturnsUnauthorized()
        {
            var controller = GetController();

            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            };

            var result = await controller.Me();

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Me_UserNotFound_ReturnsNotFound()
        {
            var controller = GetController();

            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new[]
                    {
                        new Claim("sub", "1")
                    }, "test"))
            };

            _userRepo.Setup(x => x.GetByIdAsync(1))
                     .ReturnsAsync((User?)null);

            var result = await controller.Me();

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Me_Valid_ReturnsOk()
        {
            var controller = GetController();

            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(new[]
                    {
                        new Claim("sub", "1")
                    }, "test"))
            };

            _userRepo.Setup(x => x.GetByIdAsync(1))
                     .ReturnsAsync(new User
                     {
                         Id = 1,
                         Email = "test@mail.com",
                         Name = "Test",
                         CreatedAt = DateTime.UtcNow
                     });

            var result = await controller.Me();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }
    }
}
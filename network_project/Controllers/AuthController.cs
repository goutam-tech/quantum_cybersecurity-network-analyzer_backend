using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using network_project.Dto;
using network_project.Helper;
using network_project.Interfaces;
using network_project.Models;

namespace network_project.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly ITokenRepository _tokenRepo;
    private readonly JwtHelper _jwt;

    public AuthController(
        IUserRepository userRepo,
        ITokenRepository tokenRepo,
        JwtHelper jwt)
    {
        _userRepo = userRepo;
        _tokenRepo = tokenRepo;
        _jwt = jwt;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _userRepo.GetByEmailAsync(dto.Email);
        if (existing is not null)
            return Conflict(new { message = "An account with this email already exists." });

        var user = new User
        {
            Email = dto.Email.Trim().ToLower(),
            Name = dto.Name.Trim(),
            PasswordHash = PasswordHelper.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        var (tokenString, expiresAt) = _jwt.GenerateToken(user);
        await _tokenRepo.SaveTokenAsync(new UserToken
        {
            UserId = user.Id,
            Token = tokenString,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false
        });

        return Ok(new AuthResponseDto(
            Token: tokenString,
            ExpiresAt: expiresAt,
            User: MapUser(user)));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userRepo.GetByEmailAsync(dto.Email);
        if (user is null)
            return Unauthorized(new { message = "Invalid email or password." });

        if (!PasswordHelper.VerifyPassword(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        var (tokenString, expiresAt) = _jwt.GenerateToken(user);
        await _tokenRepo.SaveTokenAsync(new UserToken
        {
            UserId = user.Id,
            Token = tokenString,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false
        });

        return Ok(new AuthResponseDto(
            Token: tokenString,
            ExpiresAt: expiresAt,
            User: MapUser(user)));
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokeDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tokenRecord = await _tokenRepo.GetTokenAsync(dto.Token);
        if (tokenRecord is null)
            return NotFound(new { message = "Token not found." });

        if (tokenRecord.IsRevoked)
            return BadRequest(new { message = "Token is already revoked." });

        await _tokenRepo.RevokeTokenAsync(dto.Token);
        return Ok(new { message = "Token revoked successfully." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                       ?? User.FindFirst("sub");

        if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { message = "Invalid token." });

        var user = await _userRepo.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new { message = "User not found." });

        return Ok(MapUser(user));
    }

    private static UserDto MapUser(User u) =>
        new(u.Id, u.Email, u.Name, u.CreatedAt);
}
using System.ComponentModel.DataAnnotations;

namespace network_project.Dto;

public class SignupDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RevokeDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public record UserDto(
    int Id,
    string Email,
    string Name,
    DateTime CreatedAt
);

public record AuthResponseDto(
    string Token,
    DateTime ExpiresAt,
    UserDto User
);
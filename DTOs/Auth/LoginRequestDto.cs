using System.ComponentModel.DataAnnotations;

namespace BlogMvc.DTOs.Auth;
public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
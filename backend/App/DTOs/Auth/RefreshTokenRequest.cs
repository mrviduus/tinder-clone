using System.ComponentModel.DataAnnotations;

namespace App.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
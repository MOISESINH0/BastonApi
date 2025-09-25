using System.ComponentModel.DataAnnotations;

namespace Baston.Api.DTOs;

public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Rol { get; set; } = string.Empty; // "Usuario" o "Contacto"
}

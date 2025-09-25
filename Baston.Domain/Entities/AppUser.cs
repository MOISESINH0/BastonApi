namespace Baston.Domain.Entities;

public class AppUser
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    // 👇 El rol lo envía Flutter: "Usuario" o "Contacto"
    public string Rol { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

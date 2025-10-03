using Baston.Api.DTOs;
using Baston.Domain.Entities;
using Baston.Infra;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Baston.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <summary>Registra un usuario nuevo (contraseña hasheada + UserTag)</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists) return Conflict(new { message = "El correo ya está registrado" });

        var user = new AppUser
        {
            Email = dto.Email.Trim().ToLowerInvariant(),
            FullName = dto.FullName.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Rol = dto.Rol.Trim()
        };

        // 🔹 Generar UserTag único (ejemplo: Moises#4821)
        user.UserTag = await GenerateUniqueUserTag(user.FullName);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Created($"/api/users/{user.UserId}", new
        {
            userId = user.UserId,
            email = user.Email,
            fullName = user.FullName,
            rol = user.Rol,
            userTag = user.UserTag
        });
    }

    /// <summary>Login: valida credenciales y devuelve un JWT</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var user = await _db.Users
            .Where(u => u.Email == dto.Email.ToLower() && u.IsActive)
            .FirstOrDefaultAsync();

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Credenciales inválidas" });

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            userId = user.UserId,
            email = user.Email,
            fullName = user.FullName,
            rol = user.Rol,
            userTag = user.UserTag,
            token
        });
    }

    // 🔹 Generar un UserTag único con formato Nombre#1234
    private async Task<string> GenerateUniqueUserTag(string fullName)
    {
        var baseName = fullName.Split(' ')[0]; // toma solo el primer nombre
        string tag;
        bool exists;

        do
        {
            var random = new Random().Next(1000, 9999); // ej: 4821
            tag = $"{baseName}#{random}";
            exists = await _db.Users.AnyAsync(u => u.UserTag == tag);
        } while (exists);

        return tag;
    }

    private string GenerateJwtToken(AppUser user)
    {
        var jwtConfig = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("fullName", user.FullName),
            new Claim("rol", user.Rol ?? "Usuario"),
            new Claim("userTag", user.UserTag ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: jwtConfig["Issuer"],
            audience: jwtConfig["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

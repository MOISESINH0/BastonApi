using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baston.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    /// <summary>
    /// Endpoint protegido. Solo accesible con un JWT válido.
    /// </summary>
    [HttpGet("secure")]
    [Authorize] // 🔒 Requiere token
    public IActionResult SecureEndpoint()
    {
        return Ok(new
        {
            message = "✅ Accediste a un endpoint protegido con JWT",
            user = User.Identity?.Name ?? "desconocido",
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }

    /// <summary>
    /// Endpoint público. No requiere token.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { message = "🌍 Este endpoint es público" });
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baston.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProtectedController : ControllerBase
{
    [Authorize] // 🔒 requiere JWT válido
    [HttpGet("hello")]
    public IActionResult Hello()
    {
        var userId = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;

        return Ok(new { message = $"Hola {email}, tu ID es {userId}" });
    }
}

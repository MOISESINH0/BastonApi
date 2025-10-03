using Baston.Domain.Entities;
using Baston.Infra;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Baston.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfianzaController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ConfianzaController(AppDbContext db)
        {
            _db = db;
        }

        // DTOs internos
        public record SendRequestDto(Guid SenderId, string ReceiverTag);
        public record RespondRequestDto(Guid RequestId, string Action);

        /// 🔹 Enviar solicitud de confianza
        [HttpPost("send")]
        public async Task<IActionResult> SendRequest([FromBody] SendRequestDto dto)
        {
            var sender = await _db.Users.FindAsync(dto.SenderId);
            if (sender == null)
                return NotFound(new { message = "El remitente no existe." });

            var receiver = await _db.Users.FirstOrDefaultAsync(u => u.UserTag == dto.ReceiverTag);
            if (receiver == null)
                return NotFound(new { message = "El usuario con ese tag no existe." });

            if (receiver.UserId == dto.SenderId)
                return BadRequest(new { message = "No puedes enviarte una solicitud a ti mismo." });

            var exists = await _db.ConfianzaRequests.AnyAsync(r =>
                r.SenderId == dto.SenderId && r.ReceiverId == receiver.UserId && r.Status == "Pending");

            if (exists)
                return BadRequest(new { message = "Ya enviaste una solicitud a este usuario." });

            var request = new ConfianzaRequest
            {
                SenderId = dto.SenderId,
                ReceiverId = receiver.UserId
            };

            _db.ConfianzaRequests.Add(request);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Solicitud enviada.",
                requestId = request.Id,
                receiver = receiver.UserTag
            });
        }

        /// 🔹 Responder (Aceptar/Rechazar) solicitud
        [HttpPost("respond")]
        public async Task<IActionResult> RespondRequest([FromBody] RespondRequestDto dto)
        {
            var request = await _db.ConfianzaRequests
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .FirstOrDefaultAsync(r => r.Id == dto.RequestId);

            if (request == null)
                return NotFound(new { message = "Solicitud no encontrada." });

            if (request.Status != "Pending")
                return BadRequest(new { message = "La solicitud ya fue respondida." });

            switch (dto.Action.ToLower())
            {
                case "accept":
                    request.Status = "Accepted";
                    break;
                case "reject":
                    request.Status = "Rejected";
                    break;
                default:
                    return BadRequest(new { message = "Acción inválida. Usa 'accept' o 'reject'." });
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"Solicitud {request.Status.ToLower()}.",
                requestId = request.Id,
                sender = new { request.Sender.FullName, request.Sender.UserTag },
                receiver = new { request.Receiver.FullName, request.Receiver.UserTag }
            });
        }

        /// 🔹 Listar solicitudes de un usuario
        [HttpGet("list/{userId}")]
        public async Task<IActionResult> ListRequests(Guid userId)
        {
            var requests = await _db.ConfianzaRequests
                .Include(r => r.Sender)
                .Include(r => r.Receiver)
                .Where(r => r.SenderId == userId || r.ReceiverId == userId)
                .OrderByDescending(r => r.SentAt)
                .ToListAsync();

            return Ok(requests.Select(r => new
            {
                r.Id,
                Sender = new { r.SenderId, r.Sender.FullName, r.Sender.UserTag },
                Receiver = new { r.ReceiverId, r.Receiver.FullName, r.Receiver.UserTag },
                r.Status,
                r.SentAt
            }));
        }
    }
}

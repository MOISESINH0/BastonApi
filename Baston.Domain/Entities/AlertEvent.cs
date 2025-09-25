using System.Text.Json;

namespace Baston.Domain.Entities;

public class AlertEvent
{
    public long AlertId { get; set; }             // bigserial
    public Guid DeviceId { get; set; }
    public DateTime T { get; set; }               // UTC
    public string Type { get; set; } = "SOS";     // 'SOS'|'CAIDA'|'BATERIA_BAJA'
    public JsonDocument? Detail { get; set; }     // jsonb
    public DateTime InsertedAt { get; set; } = DateTime.UtcNow;
}

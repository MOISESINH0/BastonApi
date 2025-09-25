namespace Baston.Domain.Entities;

public class GpsFix
{
    public long FixId { get; set; }             // bigserial
    public Guid DeviceId { get; set; }
    public DateTime T { get; set; }             // UTC
    public double Lat { get; set; }             // -90..90
    public double Lon { get; set; }             // -180..180
    public double? AltM { get; set; }           // Altitud en metros (opcional)
    public DateTime InsertedAt { get; set; } = DateTime.UtcNow;
}

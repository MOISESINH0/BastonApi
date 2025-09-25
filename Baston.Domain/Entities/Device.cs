namespace Baston.Domain.Entities;

public class Device
{
    public Guid DeviceId { get; set; }
    public Guid OwnerUserId { get; set; }
    public string BtAddress { get; set; } = default!;
    public string? Model { get; set; }
    public string? Firmware { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

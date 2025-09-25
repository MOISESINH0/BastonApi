namespace Baston.Domain.Entities;

public class KvMetaServer
{
    public Guid UserId { get; set; }
    public string K { get; set; } = default!;
    public string V { get; set; } = default!;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

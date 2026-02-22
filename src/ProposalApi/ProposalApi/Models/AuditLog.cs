namespace ProposalApi.Models;

public class AuditLog
{
    public long Id { get; set; }
    public required string Action { get; set; }
    public required string EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Detail { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

using ProposalGenerator.Web.Data;
using ProposalGenerator.Web.Models.Domain;

namespace ProposalGenerator.Web.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AppDbContext db, ILogger<AuditService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task LogAsync(string action, string entityType, int? entityId = null, string? userId = null, string? details = null)
    {
        var entry = new AuditLog
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Audit: {Action} on {EntityType} {EntityId} by {User}", action, entityType, entityId, userId);
    }
}

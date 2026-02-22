namespace ProposalGenerator.Web.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, int? entityId = null, string? userId = null, string? details = null);
}

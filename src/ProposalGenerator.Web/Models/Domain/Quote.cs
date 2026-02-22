namespace ProposalGenerator.Web.Models.Domain;

public class Quote
{
    public int Id { get; set; }
    public string QuoteNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerCompany { get; set; }
    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
    public DateTime ValidUntil { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "EUR";
    public string? Notes { get; set; }
    public string? GeneratedPdfPath { get; set; }
    public string? TemplateName { get; set; }
    public string CreatedBy { get; set; } = "system";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;

    public List<QuoteLineItem> LineItems { get; set; } = new();
}

public enum QuoteStatus
{
    Draft,
    Preview,
    Finalized,
    Approved,
    Rejected,
    Expired
}

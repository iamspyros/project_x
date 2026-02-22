namespace ProposalApi.Models;

public class Quote
{
    public int Id { get; set; }
    public required string QuoteNumber { get; set; }
    public int Version { get; set; } = 1;
    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
    public required string CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime ValidUntil { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Currency { get; set; } = "EUR";
    public string? Notes { get; set; }
    public string? PdfBlobPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinalizedAt { get; set; }

    public List<QuoteLineItem> LineItems { get; set; } = new();
}

public enum QuoteStatus
{
    Draft = 0,
    Preview = 1,
    Finalized = 2,
    Approved = 3,
    Rejected = 4,
    Expired = 5
}

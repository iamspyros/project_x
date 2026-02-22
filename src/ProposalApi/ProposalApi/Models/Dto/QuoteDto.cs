namespace ProposalApi.Models.Dto;

public class QuoteDto
{
    public int Id { get; set; }
    public required string QuoteNumber { get; set; }
    public int Version { get; set; }
    public required string Status { get; set; }
    public required string CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public DateTime ValidUntil { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Currency { get; set; }
    public string? Notes { get; set; }
    public string? PdfDownloadUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public List<QuoteLineItemDto> LineItems { get; set; } = new();
}

public class QuoteLineItemDto
{
    public required string ProductName { get; set; }
    public required string Sku { get; set; }
    public int Quantity { get; set; }
    public string? CommitmentTerm { get; set; }
    public string? BillingFrequency { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
    public required string Currency { get; set; }
}

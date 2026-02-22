namespace ProposalGenerator.Web.Models.Domain;

public class QuoteLineItem
{
    public int Id { get; set; }
    public int QuoteId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? CommitmentTerm { get; set; }
    public string? BillingFrequency { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
    public string Currency { get; set; } = "EUR";

    public Quote Quote { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

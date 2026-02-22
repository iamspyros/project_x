namespace ProposalApi.Models;

public class QuoteLineItem
{
    public int Id { get; set; }
    public int QuoteId { get; set; }
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public required string Sku { get; set; }
    public int Quantity { get; set; }
    public string? CommitmentTerm { get; set; }
    public string? BillingFrequency { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
    public required string Currency { get; set; } = "EUR";

    public Quote? Quote { get; set; }
    public Product? Product { get; set; }
}

namespace ProposalApi.Models.Dto;

public class ProductDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal UnitPrice { get; set; }
    public required string Currency { get; set; }
    public string? CommitmentTerm { get; set; }
    public string? BillingFrequency { get; set; }
}

namespace ProposalApi.Models;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Sku { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal UnitPrice { get; set; }
    public required string Currency { get; set; } = "EUR";
    public string? CommitmentTerm { get; set; }
    public string? BillingFrequency { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

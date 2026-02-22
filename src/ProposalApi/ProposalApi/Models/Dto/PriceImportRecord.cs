using CsvHelper.Configuration.Attributes;

namespace ProposalApi.Models.Dto;

public class PriceImportRecord
{
    [Name("ProductName")]
    public required string ProductName { get; set; }

    [Name("SKU")]
    public required string Sku { get; set; }

    [Name("Description")]
    public string? Description { get; set; }

    [Name("Category")]
    public string? Category { get; set; }

    [Name("UnitPrice")]
    public decimal UnitPrice { get; set; }

    [Name("Currency")]
    public string Currency { get; set; } = "EUR";

    [Name("CommitmentTerm")]
    public string? CommitmentTerm { get; set; }

    [Name("BillingFrequency")]
    public string? BillingFrequency { get; set; }
}

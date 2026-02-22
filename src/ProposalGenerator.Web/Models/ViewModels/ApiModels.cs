namespace ProposalGenerator.Web.Models.ViewModels;

public class ApiQuoteRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerCompany { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string Currency { get; set; } = "EUR";
    public string? Notes { get; set; }
    public string? TemplateName { get; set; }
    public List<ApiLineItemRequest> LineItems { get; set; } = new();
}

public class ApiLineItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? DiscountPercent { get; set; }
}

public class ApiQuoteResponse
{
    public int Id { get; set; }
    public string QuoteNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime ValidUntil { get; set; }
    public string? PdfDownloadUrl { get; set; }
    public List<ApiLineItemResponse> LineItems { get; set; } = new();
}

public class ApiLineItemResponse
{
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? CommitmentTerm { get; set; }
    public string? BillingFrequency { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class ApiProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? CommitmentTerm { get; set; }
    public string? BillingFrequency { get; set; }
}

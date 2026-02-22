using System.ComponentModel.DataAnnotations;

namespace ProposalApi.Models.Dto;

public class QuotePreviewRequest
{
    [Required]
    public required string CustomerName { get; set; }
    public string? CustomerEmail { get; set; }

    [Required]
    [MinLength(1)]
    public required List<QuoteLineItemRequest> LineItems { get; set; }
}

public class QuoteLineItemRequest
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }
}

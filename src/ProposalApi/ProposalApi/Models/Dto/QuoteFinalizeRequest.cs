using System.ComponentModel.DataAnnotations;

namespace ProposalApi.Models.Dto;

public class QuoteFinalizeRequest
{
    [Required]
    public required string CustomerName { get; set; }
    public string? CustomerEmail { get; set; }

    [Required]
    [MinLength(1)]
    public required List<QuoteLineItemRequest> LineItems { get; set; }

    public int ValidityDays { get; set; } = 30;
    public string? Notes { get; set; }
}

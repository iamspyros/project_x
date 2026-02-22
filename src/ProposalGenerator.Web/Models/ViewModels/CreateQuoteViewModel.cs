using System.ComponentModel.DataAnnotations;

namespace ProposalGenerator.Web.Models.ViewModels;

public class CreateQuoteViewModel
{
    [Required]
    [Display(Name = "Customer Name")]
    public string CustomerName { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Customer Email")]
    public string? CustomerEmail { get; set; }

    [Display(Name = "Company")]
    public string? CustomerCompany { get; set; }

    [Required]
    [Display(Name = "Valid Until")]
    [DataType(DataType.Date)]
    public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddDays(30);

    [Display(Name = "Currency")]
    public string Currency { get; set; } = "EUR";

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Template")]
    public string? TemplateName { get; set; }

    public List<QuoteLineItemInput> LineItems { get; set; } = new();
}

public class QuoteLineItemInput
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? DiscountPercent { get; set; }
}

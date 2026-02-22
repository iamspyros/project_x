using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProposalGenerator.Web.Data;
using ProposalGenerator.Web.Models.Domain;
using ProposalGenerator.Web.Models.ViewModels;
using ProposalGenerator.Web.Services;

namespace ProposalGenerator.Web.Pages.Quotes;

public class CreateModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IQuoteService _quoteService;
    private readonly IDocumentService _documentService;

    public CreateModel(AppDbContext db, IQuoteService quoteService, IDocumentService documentService)
    {
        _db = db;
        _quoteService = quoteService;
        _documentService = documentService;
    }

    [BindProperty]
    public CreateQuoteViewModel Input { get; set; } = new()
    {
        LineItems = new List<QuoteLineItemInput> { new() }
    };

    public List<Product> AvailableProducts { get; set; } = new();
    public List<string> AvailableTemplates { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadFormData();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadFormData();

        // Remove empty line items
        Input.LineItems.RemoveAll(li => li.ProductId == 0);

        if (!Input.LineItems.Any())
        {
            ModelState.AddModelError("", "Please add at least one line item.");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var userId = User.Identity?.Name ?? "web-user";
            var quote = await _quoteService.CreateQuoteAsync(Input, userId);

            TempData["Success"] = $"Quote {quote.QuoteNumber} created successfully.";
            return RedirectToPage("/Quotes/Details", new { id = quote.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error creating quote: {ex.Message}";
            return Page();
        }
    }

    private async Task LoadFormData()
    {
        AvailableProducts = await _db.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();

        AvailableTemplates = await _documentService.GetAvailableTemplatesAsync();
    }
}

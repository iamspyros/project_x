using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProposalGenerator.Web.Models.Domain;
using ProposalGenerator.Web.Services;

namespace ProposalGenerator.Web.Pages.Quotes;

public class DetailsModel : PageModel
{
    private readonly IQuoteService _quoteService;
    private readonly IDocumentService _documentService;

    public DetailsModel(IQuoteService quoteService, IDocumentService documentService)
    {
        _quoteService = quoteService;
        _documentService = documentService;
    }

    public Quote Quote { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var quote = await _quoteService.GetQuoteAsync(id);
        if (quote == null)
            return NotFound();

        Quote = quote;
        return Page();
    }

    public async Task<IActionResult> OnPostPreviewAsync(int id)
    {
        var quote = await _quoteService.GetQuoteAsync(id);
        if (quote == null)
            return NotFound();

        try
        {
            var pdf = await _documentService.GeneratePreviewPdfAsync(quote);
            return File(pdf, "application/pdf", $"{quote.QuoteNumber}_preview.pdf");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error generating preview: {ex.Message}";
            return RedirectToPage("/Quotes/Details", new { id });
        }
    }

    public async Task<IActionResult> OnPostFinalizeAsync(int id)
    {
        try
        {
            var userId = User.Identity?.Name ?? "web-user";
            var finalized = await _quoteService.FinalizeQuoteAsync(id, userId);
            TempData["Success"] = $"Quote {finalized.QuoteNumber} has been finalized. PDF is ready for download.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error finalizing quote: {ex.Message}";
        }

        return RedirectToPage("/Quotes/Details", new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _quoteService.DeleteQuoteAsync(id);
            TempData["Success"] = "Quote deleted successfully.";
            return RedirectToPage("/Quotes/Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error deleting quote: {ex.Message}";
            return RedirectToPage("/Quotes/Details", new { id });
        }
    }
}

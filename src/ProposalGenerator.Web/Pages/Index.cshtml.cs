using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProposalGenerator.Web.Data;
using ProposalGenerator.Web.Models.Domain;

namespace ProposalGenerator.Web.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public int ProductCount { get; set; }
    public int TotalQuotes { get; set; }
    public int DraftQuotes { get; set; }
    public int FinalizedQuotes { get; set; }
    public List<Quote> RecentQuotes { get; set; } = new();

    public async Task OnGetAsync()
    {
        ProductCount = await _db.Products.CountAsync(p => p.IsActive);
        TotalQuotes = await _db.Quotes.CountAsync();
        DraftQuotes = await _db.Quotes.CountAsync(q => q.Status == QuoteStatus.Draft);
        FinalizedQuotes = await _db.Quotes.CountAsync(q => q.Status == QuoteStatus.Finalized);

        RecentQuotes = await _db.Quotes
            .Include(q => q.LineItems)
            .OrderByDescending(q => q.CreatedAt)
            .Take(10)
            .ToListAsync();
    }
}

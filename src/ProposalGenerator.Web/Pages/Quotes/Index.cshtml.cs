using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProposalGenerator.Web.Data;
using ProposalGenerator.Web.Models.Domain;

namespace ProposalGenerator.Web.Pages.Quotes;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Quote> Quotes { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    public async Task OnGetAsync(string? status)
    {
        StatusFilter = status;

        IQueryable<Quote> query = _db.Quotes
            .Include(q => q.LineItems);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<QuoteStatus>(status, out var statusEnum))
        {
            query = query.Where(q => q.Status == statusEnum);
        }

        Quotes = await query
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProposalGenerator.Web.Data;
using ProposalGenerator.Web.Models.Domain;

namespace ProposalGenerator.Web.Pages.Products;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Product> Products { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? CategoryFilter { get; set; }

    public async Task OnGetAsync(string? category)
    {
        CategoryFilter = category;

        var query = _db.Products.Where(p => p.IsActive);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category == category);
        }

        Products = await query
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using ProposalApi.Data;
using ProposalApi.Models;

namespace ProposalApi.Services;

public class PricingService : IPricingService
{
    private readonly ProposalDbContext _db;

    public PricingService(ProposalDbContext db)
    {
        _db = db;
    }

    public async Task<List<Product>> GetActiveProductsAsync()
    {
        return await _db.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetProductAsync(int productId)
    {
        return await _db.Products.FindAsync(productId);
    }

    public async Task<List<Product>> GetProductsByIdsAsync(IEnumerable<int> productIds)
    {
        var ids = productIds.ToList();
        return await _db.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
    }

    public Task<decimal> CalculateLineTotalAsync(int productId, int quantity, decimal discountPercent)
    {
        // Pricing logic can be extended here (volume tiers, bundle discounts, etc.)
        return Task.FromResult(0m); // Actual calculation uses product from DB; see QuoteService
    }
}

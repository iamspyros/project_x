using ProposalApi.Models;

namespace ProposalApi.Services;

public interface IPricingService
{
    Task<List<Product>> GetActiveProductsAsync();
    Task<Product?> GetProductAsync(int productId);
    Task<List<Product>> GetProductsByIdsAsync(IEnumerable<int> productIds);
    Task<decimal> CalculateLineTotalAsync(int productId, int quantity, decimal discountPercent);
}

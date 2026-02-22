using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProposalGenerator.Web.Data;
using ProposalGenerator.Web.Models.ViewModels;

namespace ProposalGenerator.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ApiProductResponse>>> GetAll()
    {
        var products = await _db.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Select(p => new ApiProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                Description = p.Description,
                Category = p.Category,
                UnitPrice = p.UnitPrice,
                Currency = p.Currency,
                CommitmentTerm = p.CommitmentTerm,
                BillingFrequency = p.BillingFrequency
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiProductResponse>> GetById(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null || !p.IsActive)
            return NotFound();

        return Ok(new ApiProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Sku = p.Sku,
            Description = p.Description,
            Category = p.Category,
            UnitPrice = p.UnitPrice,
            Currency = p.Currency,
            CommitmentTerm = p.CommitmentTerm,
            BillingFrequency = p.BillingFrequency
        });
    }
}

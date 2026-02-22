using ProposalGenerator.Web.Models.Domain;

namespace ProposalGenerator.Web.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext context)
    {
        if (context.Products.Any())
            return;

        var products = new List<Product>
        {
            new()
            {
                Name = "Enterprise Voice Standard",
                Sku = "VOD-EV-001",
                Description = "Standard enterprise voice solution with basic features",
                Category = "Voice",
                UnitPrice = 15.00m,
                Currency = "EUR",
                CommitmentTerm = "12 months",
                BillingFrequency = "Monthly"
            },
            new()
            {
                Name = "Enterprise Voice Premium",
                Sku = "VOD-EV-002",
                Description = "Premium enterprise voice solution with advanced features",
                Category = "Voice",
                UnitPrice = 25.00m,
                Currency = "EUR",
                CommitmentTerm = "24 months",
                BillingFrequency = "Monthly"
            },
            new()
            {
                Name = "Managed SD-WAN Basic",
                Sku = "VOD-NW-001",
                Description = "Basic SD-WAN connectivity with managed service",
                Category = "Network",
                UnitPrice = 250.00m,
                Currency = "EUR",
                CommitmentTerm = "36 months",
                BillingFrequency = "Monthly"
            },
            new()
            {
                Name = "Managed SD-WAN Enterprise",
                Sku = "VOD-NW-002",
                Description = "Enterprise SD-WAN with full management and SLA",
                Category = "Network",
                UnitPrice = 500.00m,
                Currency = "EUR",
                CommitmentTerm = "36 months",
                BillingFrequency = "Monthly"
            },
            new()
            {
                Name = "Cloud Security Gateway",
                Sku = "VOD-SEC-001",
                Description = "Cloud-based security gateway with threat protection",
                Category = "Security",
                UnitPrice = 8.50m,
                Currency = "EUR",
                CommitmentTerm = "12 months",
                BillingFrequency = "Monthly"
            },
            new()
            {
                Name = "IoT Connectivity Pack",
                Sku = "VOD-IOT-001",
                Description = "IoT SIM and connectivity management platform",
                Category = "IoT",
                UnitPrice = 2.50m,
                Currency = "EUR",
                CommitmentTerm = "24 months",
                BillingFrequency = "Monthly"
            },
            new()
            {
                Name = "Unified Communications Suite",
                Sku = "VOD-UC-001",
                Description = "Integrated voice, video, and messaging platform",
                Category = "Collaboration",
                UnitPrice = 35.00m,
                Currency = "EUR",
                CommitmentTerm = "24 months",
                BillingFrequency = "Monthly"
            },
            new()
            {
                Name = "Dedicated Internet Access 100Mbps",
                Sku = "VOD-DIA-001",
                Description = "Dedicated internet access with 100Mbps symmetric bandwidth",
                Category = "Network",
                UnitPrice = 350.00m,
                Currency = "EUR",
                CommitmentTerm = "36 months",
                BillingFrequency = "Monthly"
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}

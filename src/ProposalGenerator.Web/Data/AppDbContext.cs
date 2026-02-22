using Microsoft.EntityFrameworkCore;
using ProposalGenerator.Web.Models.Domain;

namespace ProposalGenerator.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteLineItem> QuoteLineItems => Set<QuoteLineItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Sku).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 4);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.CommitmentTerm).HasMaxLength(50);
            entity.Property(e => e.BillingFrequency).HasMaxLength(50);
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QuoteNumber).IsUnique();
            entity.Property(e => e.QuoteNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CustomerEmail).HasMaxLength(200);
            entity.Property(e => e.CustomerCompany).HasMaxLength(200);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 4);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.TemplateName).HasMaxLength(200);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasMany(e => e.LineItems).WithOne(e => e.Quote).HasForeignKey(e => e.QuoteId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuoteLineItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.Sku).HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 4);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 4);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.CommitmentTerm).HasMaxLength(50);
            entity.Property(e => e.BillingFrequency).HasMaxLength(50);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(200);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}

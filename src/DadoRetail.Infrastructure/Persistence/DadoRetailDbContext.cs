using DadoRetail.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace DadoRetail.Infrastructure.Persistence;

public sealed class DadoRetailDbContext(DbContextOptions<DadoRetailDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Barcode> Barcodes => Set<Barcode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("products");
            b.HasKey(x => x.Id);
            b.Property(x => x.Sku).HasMaxLength(64).IsRequired();
            b.HasIndex(x => x.Sku).IsUnique();
            b.Property(x => x.Name).HasMaxLength(300).IsRequired();
            b.Property(x => x.UnitOfMeasure).HasMaxLength(32).IsRequired();
            b.HasMany(x => x.Barcodes).WithOne(x => x.Product).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Barcode>(b =>
        {
            b.ToTable("barcodes");
            b.HasKey(x => x.Id);
            b.Property(x => x.Value).HasMaxLength(64).IsRequired();
            b.HasIndex(x => x.Value).IsUnique();
        });
    }
}

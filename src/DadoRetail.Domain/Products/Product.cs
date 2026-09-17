using DadoRetail.Domain.Common;

namespace DadoRetail.Domain.Products;

public sealed class Product : Entity
{
    private Product() { }

    public Product(string sku, string name, string unitOfMeasure)
    {
        SetSku(sku);
        Rename(name);
        UnitOfMeasure = string.IsNullOrWhiteSpace(unitOfMeasure) ? throw new ArgumentException("Unit is required.") : unitOfMeasure.Trim();
    }

    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string UnitOfMeasure { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public bool IsWeighted { get; private set; }
    public ICollection<Barcode> Barcodes { get; private set; } = new List<Barcode>();

    public void Rename(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Product name is required.") : name.Trim();
        MarkUpdated();
    }

    public void SetSku(string sku)
    {
        Sku = string.IsNullOrWhiteSpace(sku) ? throw new ArgumentException("SKU is required.") : sku.Trim();
        MarkUpdated();
    }

    public void SetWeighted(bool value) { IsWeighted = value; MarkUpdated(); }
    public void Deactivate() { IsActive = false; MarkUpdated(); }
}

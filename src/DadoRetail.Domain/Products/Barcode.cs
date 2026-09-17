using DadoRetail.Domain.Common;

namespace DadoRetail.Domain.Products;

public sealed class Barcode : Entity
{
    private Barcode() { }

    public Barcode(Guid productId, string value, bool isPrimary = false)
    {
        ProductId = productId;
        Value = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Barcode is required.") : value.Trim();
        IsPrimary = isPrimary;
    }

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public bool IsPrimary { get; private set; }
}

using DadoRetail.Domain.Common;

namespace DadoRetail.Domain.Pricing;

public sealed class Price : Entity
{
    private Price() { }
    public Price(Guid productId, Guid storeId, decimal amount, DateTime validFromUtc, DateTime? validToUtc=null)
    { if(amount<0) throw new ArgumentOutOfRangeException(nameof(amount)); ProductId=productId; StoreId=storeId; Amount=amount; ValidFromUtc=validFromUtc; ValidToUtc=validToUtc; }
    public Guid ProductId { get; private set; }
    public Guid StoreId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime ValidFromUtc { get; private set; }
    public DateTime? ValidToUtc { get; private set; }
    public bool IsActive { get; private set; } = true;
}

public sealed class Promotion : Entity
{
    private Promotion() { }
    public Promotion(string name, DateTime fromUtc, DateTime toUtc, decimal discountPercent)
    { if(discountPercent<0||discountPercent>100) throw new ArgumentOutOfRangeException(nameof(discountPercent)); Name=name; FromUtc=fromUtc; ToUtc=toUtc; DiscountPercent=discountPercent; }
    public string Name { get; private set; } = string.Empty;
    public DateTime FromUtc { get; private set; }
    public DateTime ToUtc { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public bool IsActive { get; private set; } = true;
}

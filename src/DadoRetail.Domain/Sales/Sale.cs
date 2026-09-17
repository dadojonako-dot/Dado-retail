using DadoRetail.Domain.Common;

namespace DadoRetail.Domain.Sales;

public enum SaleStatus { Draft, AwaitingPayment, Paid, AwaitingFiscalization, Completed, Cancelled, Refunded }

public sealed class Sale : Entity
{
    private Sale() { }
    public Sale(Guid storeId, Guid cashRegisterId, Guid cashierId)
    { StoreId = storeId; CashRegisterId = cashRegisterId; CashierId = cashierId; }
    public Guid StoreId { get; private set; }
    public Guid CashRegisterId { get; private set; }
    public Guid CashierId { get; private set; }
    public SaleStatus Status { get; private set; } = SaleStatus.Draft;
    public ICollection<SaleItem> Items { get; private set; } = new List<SaleItem>();
    public decimal Total => Items.Sum(x => x.Quantity * x.UnitPrice - x.DiscountAmount);
    public void AddItem(Guid productId, decimal quantity, decimal unitPrice) { if (Status != SaleStatus.Draft) throw new InvalidOperationException("Sale is not editable."); if (quantity <= 0 || unitPrice < 0) throw new ArgumentOutOfRangeException(); Items.Add(new SaleItem(Id, productId, quantity, unitPrice)); MarkUpdated(); }
    public void AwaitPayment() { if (Items.Count == 0) throw new InvalidOperationException("Empty sale."); Status = SaleStatus.AwaitingPayment; MarkUpdated(); }
    public void MarkPaid() { if (Status is not (SaleStatus.AwaitingPayment or SaleStatus.Draft)) throw new InvalidOperationException("Invalid payment transition."); Status = SaleStatus.Paid; MarkUpdated(); }
    public void AwaitFiscalization() { if (Status != SaleStatus.Paid) throw new InvalidOperationException("Sale must be paid."); Status = SaleStatus.AwaitingFiscalization; MarkUpdated(); }
    public void Complete() { if (Status != SaleStatus.AwaitingFiscalization) throw new InvalidOperationException("Sale must be fiscalized first."); Status = SaleStatus.Completed; MarkUpdated(); }
}

public sealed class SaleItem : Entity
{
    private SaleItem() { }
    internal SaleItem(Guid saleId, Guid productId, decimal quantity, decimal unitPrice) { SaleId=saleId; ProductId=productId; Quantity=quantity; UnitPrice=unitPrice; }
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
}

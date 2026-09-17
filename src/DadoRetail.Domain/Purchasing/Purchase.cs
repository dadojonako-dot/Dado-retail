using DadoRetail.Domain.Common;

namespace DadoRetail.Domain.Purchasing;

public enum DocumentStatus { Draft, Posted, Cancelled }

public sealed class Purchase : Entity
{
    private Purchase() { }
    public Purchase(Guid supplierId, Guid warehouseId, string documentNumber)
    { SupplierId = supplierId; WarehouseId = warehouseId; DocumentNumber = documentNumber.Trim(); }

    public Guid SupplierId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string DocumentNumber { get; private set; } = null!;
    public DocumentStatus Status { get; private set; } = DocumentStatus.Draft;
    public DateTime? PostedAtUtc { get; private set; }
    public ICollection<PurchaseItem> Items { get; private set; } = new List<PurchaseItem>();

    public void AddItem(Guid productId, decimal quantity, decimal purchasePrice, DateOnly? expiryDate = null)
    {
        if (Status != DocumentStatus.Draft) throw new InvalidOperationException("Posted document cannot be edited.");
        if (quantity <= 0 || purchasePrice < 0) throw new ArgumentOutOfRangeException();
        Items.Add(new PurchaseItem(Id, productId, quantity, purchasePrice, expiryDate));
        MarkUpdated();
    }

    public void MarkPosted()
    {
        if (Status != DocumentStatus.Draft || Items.Count == 0) throw new InvalidOperationException("Document cannot be posted.");
        Status = DocumentStatus.Posted; PostedAtUtc = DateTime.UtcNow; MarkUpdated();
    }
}

public sealed class PurchaseItem : Entity
{
    private PurchaseItem() { }
    internal PurchaseItem(Guid purchaseId, Guid productId, decimal quantity, decimal purchasePrice, DateOnly? expiryDate)
    { PurchaseId = purchaseId; ProductId = productId; Quantity = quantity; PurchasePrice = purchasePrice; ExpiryDate = expiryDate; }
    public Guid PurchaseId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
}

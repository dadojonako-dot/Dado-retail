using DadoRetail.Domain.Common;

namespace DadoRetail.Domain.Inventory;

public sealed class StockBalance : Entity
{
    private StockBalance() { }
    public StockBalance(Guid warehouseId, Guid productId) { WarehouseId = warehouseId; ProductId = productId; }
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public void Apply(decimal delta) { Quantity += delta; MarkUpdated(); }
}

public enum StockMovementType { Receipt, Sale, CustomerReturn, SupplierReturn, TransferIn, TransferOut, WriteOff, Adjustment }

public sealed class StockMovement : Entity
{
    private StockMovement() { }
    public StockMovement(Guid warehouseId, Guid productId, decimal quantity, StockMovementType type, Guid documentId)
    { WarehouseId = warehouseId; ProductId = productId; Quantity = quantity; Type = type; DocumentId = documentId; }
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public StockMovementType Type { get; private set; }
    public Guid DocumentId { get; private set; }
}

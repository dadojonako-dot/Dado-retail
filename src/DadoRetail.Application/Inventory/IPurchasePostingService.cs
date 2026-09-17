using DadoRetail.Domain.Purchasing;

namespace DadoRetail.Application.Inventory;

public interface IPurchasePostingService
{
    Task PostAsync(Guid purchaseId, Guid userId, CancellationToken cancellationToken = default);
}

public interface IPurchaseRepository
{
    Task<Purchase?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IStockLedger
{
    Task ApplyReceiptAsync(Guid warehouseId, Guid productId, decimal quantity, Guid documentId, CancellationToken cancellationToken = default);
}

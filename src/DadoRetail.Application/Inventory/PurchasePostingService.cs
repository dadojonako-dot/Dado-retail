namespace DadoRetail.Application.Inventory;

public sealed class PurchasePostingService(IPurchaseRepository purchases, IStockLedger stock) : IPurchasePostingService
{
    public async Task PostAsync(Guid purchaseId, Guid userId, CancellationToken cancellationToken = default)
    {
        var purchase = await purchases.GetAsync(purchaseId, cancellationToken)
            ?? throw new KeyNotFoundException("Purchase document not found.");

        foreach (var item in purchase.Items)
            await stock.ApplyReceiptAsync(purchase.WarehouseId, item.ProductId, item.Quantity, purchase.Id, cancellationToken);

        purchase.MarkPosted();
        await purchases.SaveChangesAsync(cancellationToken);
    }
}

namespace DadoRetail.Application.Sales;

public interface ISaleStockService
{
    Task DeductAsync(Guid warehouseId, Guid saleId, CancellationToken ct = default);
    Task RestoreAsync(Guid warehouseId, Guid saleId, CancellationToken ct = default);
}

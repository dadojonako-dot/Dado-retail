using DadoRetail.Domain.Fiscalization;

namespace DadoRetail.Application.Sales;

public interface ISaleCompletionService
{
    Task<SaleCompletionResult> CompleteAsync(Guid saleId, Guid warehouseId, CancellationToken ct = default);
}

public sealed record SaleCompletionResult(Guid SaleId, string Status, string? FiscalNumber);

public interface IFiscalizationGateway
{
    Task<FiscalizationResult> FiscalizeAsync(Guid saleId, decimal amount, CancellationToken ct = default);
}

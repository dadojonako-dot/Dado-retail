using DadoRetail.Domain.Common;

namespace DadoRetail.Domain.Fiscalization;

public enum FiscalStatus { Pending, Fiscalized, Failed, RetryRequired, LawfulNonFiscal }

public sealed class FiscalTransaction : Entity
{
    private FiscalTransaction() { }
    public FiscalTransaction(Guid saleId) => SaleId = saleId;
    public Guid SaleId { get; private set; }
    public FiscalStatus Status { get; private set; } = FiscalStatus.Pending;
    public string? ProviderReceiptId { get; private set; }
    public string? FiscalNumber { get; private set; }
    public string? Error { get; private set; }
}

public interface IFiscalizationProvider
{
    Task<FiscalizationResult> FiscalizeAsync(Guid saleId, decimal amount, CancellationToken ct = default);
    Task<FiscalizationResult> RefundAsync(Guid saleId, decimal amount, CancellationToken ct = default);
}

public sealed record FiscalizationResult(bool Success, string? ReceiptId, string? FiscalNumber, string? Error);

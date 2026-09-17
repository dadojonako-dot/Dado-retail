using DadoRetail.Domain.Common;

namespace DadoRetail.Domain.Payments;

public enum PaymentMethod { Cash, Card, Qr, Bonus, Mixed }
public enum PaymentStatus { Created, WaitingPayment, Paid, Failed, Expired, Cancelled, Refunded }

public sealed class Payment : Entity
{
    private Payment() { }
    public Payment(Guid saleId, decimal amount, PaymentMethod method)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        SaleId = saleId; Amount = amount; Method = method;
    }
    public Guid SaleId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Created;
    public string? ProviderTransactionId { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public void WaitForProvider(string transactionId, DateTime? expiresAtUtc){if(Status!=PaymentStatus.Created)throw new InvalidOperationException();ProviderTransactionId=transactionId;ExpiresAtUtc=expiresAtUtc;Status=PaymentStatus.WaitingPayment;MarkUpdated();}
    public void MarkPaid(){if(Status is not (PaymentStatus.Created or PaymentStatus.WaitingPayment))throw new InvalidOperationException();Status=PaymentStatus.Paid;MarkUpdated();}
    public void MarkProviderStatus(PaymentStatus status){if(status is PaymentStatus.Paid){MarkPaid();return;}if(status is not (PaymentStatus.Failed or PaymentStatus.Expired or PaymentStatus.Cancelled))throw new InvalidOperationException();Status=status;MarkUpdated();}
}

public interface IPaymentProvider
{
    Task<PaymentProviderResult> CreateAsync(Guid paymentId, decimal amount, CancellationToken ct = default);
    Task<PaymentProviderStatus> GetStatusAsync(string providerTransactionId, CancellationToken ct = default);
    Task CancelAsync(string providerTransactionId, CancellationToken ct = default);
}

public sealed record PaymentProviderResult(string TransactionId, string? QrPayload, DateTime? ExpiresAtUtc);
public sealed record PaymentProviderStatus(PaymentStatus Status, string? AuthorizationCode = null);

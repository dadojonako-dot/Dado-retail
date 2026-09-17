using DadoRetail.Api.Security;
using DadoRetail.Application.Security;
using DadoRetail.Domain.Payments;
using DadoRetail.Domain.Sales;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DadoRetail.Api.Controllers;

[ApiController, Route("api/v1/payments")]
public sealed class PaymentsController(DadoRetailDbContext db, IEnumerable<IPaymentProvider> providers) : ControllerBase
{
    [HttpPost("{id:guid}/start"), HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
    {
        var payment = await db.Payments.FirstOrDefaultAsync(x=>x.Id==id, ct);
        if (payment is null) return NotFound();
        if (payment.Method == PaymentMethod.Cash) return Ok(new { payment.Id, payment.Status, requiresProvider=false });
        if(payment.Status!=PaymentStatus.Created)return Conflict(new{message="Платеж уже запущен",payment.Status});
        var provider = providers.FirstOrDefault();
        if (provider is null) return StatusCode(503, new { message="Провайдер эквайринга/QR не настроен" });
        var result = await provider.CreateAsync(payment.Id, payment.Amount, ct);
        payment.WaitForProvider(result.TransactionId,result.ExpiresAtUtc);await db.SaveChangesAsync(ct);
        return Ok(new { payment.Id, payment.Status, providerTransactionId=result.TransactionId, qrPayload=result.QrPayload, expiresAtUtc=result.ExpiresAtUtc });
    }

    [HttpGet("{id:guid}/status"), HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> Status(Guid id, CancellationToken ct)
    {
        var payment = await db.Payments.FirstOrDefaultAsync(x=>x.Id==id, ct);
        if (payment is null) return NotFound();

        if(payment.Status==PaymentStatus.WaitingPayment&&payment.ProviderTransactionId is not null)
        {
            var provider=providers.FirstOrDefault();
            if(provider is null)return StatusCode(503,new{message="Провайдер эквайринга/QR не настроен"});
            var remote=await provider.GetStatusAsync(payment.ProviderTransactionId,ct);
            if(remote.Status!=PaymentStatus.WaitingPayment)payment.MarkProviderStatus(remote.Status);
        }

        if(payment.Status==PaymentStatus.Paid)
        {
            var sale=await db.Sales.Include(x=>x.Items).FirstOrDefaultAsync(x=>x.Id==payment.SaleId,ct);
            if(sale is not null&&sale.Status==SaleStatus.AwaitingPayment)
            {
                // The current payment may have just changed to Paid in memory and is not yet
                // visible to the database aggregate below. Count other persisted paid payments
                // and add the tracked payment explicitly to avoid leaving the sale AwaitingPayment.
                var otherPaid=await db.Payments
                    .Where(x=>x.SaleId==sale.Id&&x.Id!=payment.Id&&x.Status==PaymentStatus.Paid)
                    .SumAsync(x=>(decimal?)x.Amount,ct)??0;
                var paid=otherPaid+payment.Amount;
                if(paid>=sale.Total)sale.MarkPaid();
            }
        }

        await db.SaveChangesAsync(ct);
        return Ok(new { payment.Id, payment.Status, payment.Amount, payment.Method, payment.ProviderTransactionId });
    }
}

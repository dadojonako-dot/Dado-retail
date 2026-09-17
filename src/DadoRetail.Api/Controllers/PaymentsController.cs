using DadoRetail.Api.Security;
using DadoRetail.Application.Security;
using DadoRetail.Domain.Payments;
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
        var provider = providers.FirstOrDefault();
        if (provider is null) return StatusCode(503, new { message="Провайдер эквайринга/QR не настроен" });
        var result = await provider.CreateAsync(payment.Id, payment.Amount, ct);
        return Ok(new { payment.Id, providerTransactionId=result.TransactionId, qrPayload=result.QrPayload, expiresAtUtc=result.ExpiresAtUtc });
    }

    [HttpGet("{id:guid}/status"), HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> Status(Guid id, CancellationToken ct)
    {
        var payment = await db.Payments.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id, ct);
        return payment is null ? NotFound() : Ok(new { payment.Id, payment.Status, payment.Amount, payment.Method, payment.ProviderTransactionId });
    }
}

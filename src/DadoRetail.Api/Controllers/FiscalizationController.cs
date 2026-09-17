using DadoRetail.Api.Security;
using DadoRetail.Application.Security;
using DadoRetail.Domain.Fiscalization;
using DadoRetail.Domain.Sales;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DadoRetail.Api.Controllers;

[ApiController, Route("api/v1/sales/{saleId:guid}/fiscalization")]
public sealed class FiscalizationController(DadoRetailDbContext db, IEnumerable<IFiscalizationProvider> providers) : ControllerBase
{
    [HttpPost, HasPermission(Permissions.FiscalSale)]
    public async Task<IActionResult> Fiscalize(Guid saleId, CancellationToken ct)
    {
        var sale = await db.Sales.Include(x=>x.Items).FirstOrDefaultAsync(x=>x.Id==saleId, ct);
        if (sale is null) return NotFound();
        if (sale.Status != SaleStatus.Paid) return BadRequest(new { message="Сначала должна быть подтверждена оплата" });
        var provider = providers.FirstOrDefault();
        if (provider is null) return StatusCode(503, new { message="Фискальный провайдер не настроен" });
        sale.AwaitFiscalization();
        var tx = new FiscalTransaction(sale.Id); db.Set<FiscalTransaction>().Add(tx); await db.SaveChangesAsync(ct);
        var result = await provider.FiscalizeAsync(sale.Id, sale.Total, ct);
        if (!result.Success) return StatusCode(502, new { message="Ошибка фискализации", result.Error });
        sale.Complete(); await db.SaveChangesAsync(ct);
        return Ok(new { sale.Id, sale.Status, result.ReceiptId, result.FiscalNumber });
    }
}

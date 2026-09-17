using DadoRetail.Api.Security;
using DadoRetail.Application.Security;
using DadoRetail.Domain.Fiscalization;
using DadoRetail.Domain.Inventory;
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
        if (sale.Status is not (SaleStatus.Paid or SaleStatus.AwaitingFiscalization)) return BadRequest(new { message="Сначала должна быть подтверждена оплата" });
        var provider = providers.FirstOrDefault();
        if (provider is null) return StatusCode(503, new { message="Фискальный провайдер не настроен" });
        var tx = await db.FiscalTransactions.OrderByDescending(x=>x.CreatedAtUtc).FirstOrDefaultAsync(x=>x.SaleId==sale.Id,ct);
        if(sale.Status==SaleStatus.Paid){sale.AwaitFiscalization();tx=new FiscalTransaction(sale.Id);db.FiscalTransactions.Add(tx);await db.SaveChangesAsync(ct);}
        var result = await provider.FiscalizeAsync(sale.Id, sale.Total, ct);
        if (!result.Success){tx!.MarkFailed(result.Error??"Fiscalization failed");await db.SaveChangesAsync(ct);return StatusCode(502, new { message="Ошибка фискализации", result.Error });}
        await using var dbtx=await db.Database.BeginTransactionAsync(ct);
        tx!.MarkFiscalized(result.ReceiptId??string.Empty,result.FiscalNumber??string.Empty);
        foreach(var g in sale.Items.GroupBy(x=>x.ProductId)){
            var quantity=g.Sum(x=>x.Quantity);
            var balance=await db.StockBalances.FirstOrDefaultAsync(x=>x.WarehouseId==sale.WarehouseId&&x.ProductId==g.Key,ct);
            if(balance is null||balance.Quantity<quantity){await dbtx.RollbackAsync(ct);return Conflict(new{message="Недостаточный остаток при закрытии чека",productId=g.Key});}
            balance.Apply(-quantity);db.StockMovements.Add(new StockMovement(sale.WarehouseId,g.Key,-quantity,StockMovementType.Sale,sale.Id));
        }
        sale.Complete();await db.SaveChangesAsync(ct);await dbtx.CommitAsync(ct);
        return Ok(new { sale.Id, sale.Status, result.ReceiptId, result.FiscalNumber });
    }
}

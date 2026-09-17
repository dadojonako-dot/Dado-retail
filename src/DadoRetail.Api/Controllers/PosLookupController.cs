using DadoRetail.Api.Security;
using DadoRetail.Application.Security;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DadoRetail.Api.Controllers;

[ApiController, Route("api/v1/pos")]
public sealed class PosLookupController(DadoRetailDbContext db) : ControllerBase
{
    [HttpGet("lookup/{barcode}"), HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> Lookup(string barcode, [FromQuery] Guid warehouseId, CancellationToken ct)
    {
        var result = await db.Barcodes.AsNoTracking().Where(x=>x.Value==barcode)
            .Select(x=>new { x.Product.Id, x.Product.Sku, x.Product.Name, x.Product.UnitOfMeasure, Barcode=x.Value })
            .FirstOrDefaultAsync(ct);
        if (result is null) return NotFound(new { message="Товар не найден" });
        var stock = await db.StockBalances.AsNoTracking().Where(x=>x.WarehouseId==warehouseId && x.ProductId==result.Id).Select(x=>x.Quantity).FirstOrDefaultAsync(ct);
        return Ok(new { product=result, stock });
    }
}

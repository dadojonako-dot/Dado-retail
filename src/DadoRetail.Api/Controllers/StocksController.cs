using DadoRetail.Api.Security;
using DadoRetail.Application.Security;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DadoRetail.Api.Controllers;

[ApiController, Route("api/v1/stocks")]
public sealed class StocksController(DadoRetailDbContext db) : ControllerBase
{
    [HttpGet, HasPermission(Permissions.ProductView)]
    public async Task<IActionResult> Get([FromQuery] Guid? warehouseId, [FromQuery] Guid? productId, CancellationToken ct)
    {
        var q = db.StockBalances.AsNoTracking().AsQueryable();
        if (warehouseId.HasValue) q = q.Where(x=>x.WarehouseId==warehouseId);
        if (productId.HasValue) q = q.Where(x=>x.ProductId==productId);
        return Ok(await q.Take(1000).ToListAsync(ct));
    }

    [HttpGet("movements"), HasPermission(Permissions.ProductView)]
    public async Task<IActionResult> Movements([FromQuery] Guid? productId, CancellationToken ct)
    {
        var q = db.StockMovements.AsNoTracking().AsQueryable();
        if (productId.HasValue) q = q.Where(x=>x.ProductId==productId);
        return Ok(await q.OrderByDescending(x=>x.CreatedAtUtc).Take(1000).ToListAsync(ct));
    }
}

using DadoRetail.Api.Security;
using DadoRetail.Application.Security;
using DadoRetail.Domain.Pricing;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DadoRetail.Api.Controllers;
[ApiController,Route("api/v1/prices")]
public sealed class PricesController(DadoRetailDbContext db):ControllerBase{
[HttpGet,HasPermission(Permissions.ProductView)]public async Task<IActionResult> List([FromQuery]Guid? storeId,[FromQuery]Guid? productId,CancellationToken ct){var q=db.Prices.AsNoTracking().AsQueryable();if(storeId.HasValue)q=q.Where(x=>x.StoreId==storeId);if(productId.HasValue)q=q.Where(x=>x.ProductId==productId);return Ok(await q.OrderByDescending(x=>x.ValidFromUtc).Take(1000).ToListAsync(ct));}
[HttpPost,HasPermission(Permissions.PriceEdit)]public async Task<IActionResult> Create(CreatePriceRequest r,CancellationToken ct){if(r.Amount<0)return BadRequest();var p=new Price(r.ProductId,r.StoreId,r.Amount,r.ValidFromUtc??DateTime.UtcNow,r.ValidToUtc);db.Prices.Add(p);await db.SaveChangesAsync(ct);return Ok(p);}}
public sealed record CreatePriceRequest(Guid ProductId,Guid StoreId,decimal Amount,DateTime? ValidFromUtc,DateTime? ValidToUtc);

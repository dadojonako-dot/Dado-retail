using DadoRetail.Api.Security;
using DadoRetail.Application.Security;
using DadoRetail.Domain.Pricing;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DadoRetail.Api.Controllers;
[ApiController,Route("api/v1/promotions")]
public sealed class PromotionsController(DadoRetailDbContext db):ControllerBase{
[HttpGet,HasPermission(Permissions.ProductView)]public async Task<IActionResult> List(CancellationToken ct)=>Ok(await db.Promotions.AsNoTracking().OrderByDescending(x=>x.FromUtc).Take(500).ToListAsync(ct));
[HttpPost,HasPermission(Permissions.PriceEdit)]public async Task<IActionResult>Create(CreatePromotionRequest r,CancellationToken ct){if(r.ToUtc<=r.FromUtc)return BadRequest(new{message="Неверный период акции"});var p=new Promotion(r.Name,r.FromUtc,r.ToUtc,r.DiscountPercent);db.Promotions.Add(p);await db.SaveChangesAsync(ct);return Ok(p);}}
public sealed record CreatePromotionRequest(string Name,DateTime FromUtc,DateTime ToUtc,decimal DiscountPercent);

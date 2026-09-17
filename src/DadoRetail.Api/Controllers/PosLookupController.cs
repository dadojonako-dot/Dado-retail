using DadoRetail.Api.Security;
using DadoRetail.Application.Security;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DadoRetail.Api.Controllers;
[ApiController,Route("api/v1/pos")]
public sealed class PosLookupController(DadoRetailDbContext db):ControllerBase{
[HttpGet("lookup/{barcode}"),HasPermission(Permissions.SaleCreate)]
public async Task<IActionResult> Lookup(string barcode,[FromQuery]Guid warehouseId,[FromQuery]Guid storeId,CancellationToken ct){
var product=await db.Barcodes.AsNoTracking().Where(x=>x.Value==barcode).Select(x=>new{x.Product.Id,x.Product.Sku,x.Product.Name,x.Product.UnitOfMeasure,Barcode=x.Value}).FirstOrDefaultAsync(ct);if(product is null)return NotFound(new{message="Товар не найден"});
var stock=await db.StockBalances.AsNoTracking().Where(x=>x.WarehouseId==warehouseId&&x.ProductId==product.Id).Select(x=>x.Quantity).FirstOrDefaultAsync(ct);
var now=DateTime.UtcNow;var price=await db.Prices.AsNoTracking().Where(x=>x.ProductId==product.Id&&x.StoreId==storeId&&x.IsActive&&x.ValidFromUtc<=now&&(x.ValidToUtc==null||x.ValidToUtc>now)).OrderByDescending(x=>x.ValidFromUtc).Select(x=>(decimal?)x.Amount).FirstOrDefaultAsync(ct);
if(price is null)return Conflict(new{message="Для товара не установлена действующая цена"});
return Ok(new{product,stock,price});}}

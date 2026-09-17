using DadoRetail.Api.Security;
using DadoRetail.Application.Security;
using DadoRetail.Domain.Products;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DadoRetail.Api.Controllers;

[ApiController, Route("api/v1/products")]
public sealed class ProductsController(DadoRetailDbContext db) : ControllerBase
{
    [HttpGet, HasPermission(Permissions.ProductView)]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await db.Products.AsNoTracking().OrderBy(x=>x.Name).Take(500).ToListAsync(ct));

    [HttpGet("by-barcode/{barcode}"), HasPermission(Permissions.ProductView)]
    public async Task<IActionResult> ByBarcode(string barcode, CancellationToken ct)
    {
        var item = await db.Barcodes.AsNoTracking().Where(x=>x.Value==barcode).Select(x=>x.Product).FirstOrDefaultAsync(ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost, HasPermission(Permissions.ProductCreate)]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken ct)
    {
        var product = new Product(request.Sku, request.Name, request.UnitOfMeasure);
        db.Products.Add(product);
        foreach (var code in request.Barcodes.Distinct()) db.Barcodes.Add(new Barcode(product.Id, code));
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(ByBarcode), new { barcode = request.Barcodes.FirstOrDefault() ?? product.Sku }, new { product.Id });
    }
}

public sealed record CreateProductRequest(string Sku, string Name, string UnitOfMeasure, IReadOnlyCollection<string> Barcodes);

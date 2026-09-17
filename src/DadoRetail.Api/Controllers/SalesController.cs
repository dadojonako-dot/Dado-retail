using System.Security.Claims;
using DadoRetail.Api.Security;
using DadoRetail.Application.Security;
using DadoRetail.Domain.Payments;
using DadoRetail.Domain.Sales;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DadoRetail.Api.Controllers;

[ApiController, Route("api/v1/sales")]
public sealed class SalesController(DadoRetailDbContext db) : ControllerBase
{
    [HttpPost, HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> Create(CreateSaleRequest request, CancellationToken ct)
    {
        var cashierId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);
        var sale = new Sale(request.StoreId, request.CashRegisterId, cashierId);
        db.Sales.Add(sale); await db.SaveChangesAsync(ct);
        return Ok(new { sale.Id, sale.Status });
    }

    [HttpPost("{id:guid}/items/by-barcode"), HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> AddByBarcode(Guid id, AddBarcodeItemRequest request, CancellationToken ct)
    {
        var sale = await db.Sales.Include(x=>x.Items).FirstOrDefaultAsync(x=>x.Id==id, ct);
        if (sale is null) return NotFound();
        var product = await db.Barcodes.Where(x=>x.Value==request.Barcode).Select(x=>x.Product).FirstOrDefaultAsync(ct);
        if (product is null) return NotFound(new { message="Штрихкод не найден" });
        sale.AddItem(product.Id, request.Quantity <= 0 ? 1 : request.Quantity, request.UnitPrice);
        await db.SaveChangesAsync(ct);
        return Ok(new { sale.Id, sale.Total, sale.Items });
    }

    [HttpPost("{id:guid}/payments"), HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> CreatePayment(Guid id, CreatePaymentRequest request, CancellationToken ct)
    {
        var sale = await db.Sales.Include(x=>x.Items).FirstOrDefaultAsync(x=>x.Id==id, ct);
        if (sale is null) return NotFound();
        if (sale.Total <= 0) return BadRequest(new { message="Пустой чек" });
        var payment = new Payment(sale.Id, request.Amount <= 0 ? sale.Total : request.Amount, request.Method);
        db.Payments.Add(payment); await db.SaveChangesAsync(ct);
        return Ok(new { payment.Id, payment.Amount, payment.Method, payment.Status });
    }

    [HttpGet("{id:guid}"), HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var sale = await db.Sales.AsNoTracking().Include(x=>x.Items).FirstOrDefaultAsync(x=>x.Id==id, ct);
        return sale is null ? NotFound() : Ok(new { sale.Id, sale.StoreId, sale.CashRegisterId, sale.CashierId, sale.Status, sale.Total, sale.Items });
    }
}

public sealed record CreateSaleRequest(Guid StoreId, Guid CashRegisterId);
public sealed record AddBarcodeItemRequest(string Barcode, decimal Quantity, decimal UnitPrice);
public sealed record CreatePaymentRequest(PaymentMethod Method, decimal Amount);

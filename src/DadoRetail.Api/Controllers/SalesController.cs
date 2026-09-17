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
    [HttpGet, HasPermission(Permissions.ReportView)]
    public async Task<IActionResult> List([FromQuery] Guid? storeId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var q = db.Sales.AsNoTracking().AsQueryable();
        if (storeId.HasValue) q = q.Where(x => x.StoreId == storeId.Value);
        if (from.HasValue) q = q.Where(x => x.CreatedAtUtc >= from.Value);
        if (to.HasValue) q = q.Where(x => x.CreatedAtUtc < to.Value);

        var rows = await q.OrderByDescending(x => x.CreatedAtUtc).Take(1000)
            .Select(x => new
            {
                x.Id,
                x.StoreId,
                x.WarehouseId,
                x.CashRegisterId,
                x.CashierId,
                x.Status,
                Items = x.Items.Count,
                Total = x.Items.Sum(i => i.Quantity * i.UnitPrice - i.DiscountAmount),
                Paid = db.Payments.Where(p => p.SaleId == x.Id && p.Status == PaymentStatus.Paid).Sum(p => (decimal?)p.Amount) ?? 0,
                x.CreatedAtUtc
            }).ToListAsync(ct);

        return Ok(rows);
    }

    [HttpPost, HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> Create(CreateSaleRequest r, CancellationToken ct)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var cashierId)) return Unauthorized();
        var register = await db.CashRegisters.AsNoTracking().FirstOrDefaultAsync(x => x.Id == r.CashRegisterId && x.StoreId == r.StoreId && x.IsActive, ct);
        if (register is null) return BadRequest(new { message = "Касса не найдена или неактивна" });
        var warehouse = await db.Warehouses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == r.WarehouseId && x.StoreId == r.StoreId, ct);
        if (warehouse is null) return BadRequest(new { message = "Склад не относится к магазину" });
        var sale = new Sale(r.StoreId, r.WarehouseId, r.CashRegisterId, cashierId);
        db.Sales.Add(sale);
        await db.SaveChangesAsync(ct);
        return Ok(new { sale.Id, sale.Status });
    }

    [HttpPost("{id:guid}/items/by-barcode"), HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> AddByBarcode(Guid id, AddBarcodeItemRequest r, CancellationToken ct)
    {
        var sale = await db.Sales.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (sale is null) return NotFound();
        if (sale.Status != SaleStatus.Draft) return Conflict(new { message = "Чек уже передан к оплате" });
        var productId = await db.Barcodes.Where(x => x.Value == r.Barcode).Select(x => (Guid?)x.ProductId).FirstOrDefaultAsync(ct);
        if (productId is null) return NotFound(new { message = "Штрихкод не найден" });
        var quantity = r.Quantity <= 0 ? 1 : r.Quantity;
        var inCart = sale.Items.Where(x => x.ProductId == productId.Value).Sum(x => x.Quantity);
        var stock = await db.StockBalances.Where(x => x.WarehouseId == sale.WarehouseId && x.ProductId == productId.Value).Select(x => (decimal?)x.Quantity).FirstOrDefaultAsync(ct) ?? 0;
        if (stock < inCart + quantity) return Conflict(new { message = "Недостаточно товара на складе", available = stock - inCart });
        var now = DateTime.UtcNow;
        var price = await db.Prices.Where(x => x.ProductId == productId && x.StoreId == sale.StoreId && x.IsActive && x.ValidFromUtc <= now && (x.ValidToUtc == null || x.ValidToUtc > now)).OrderByDescending(x => x.ValidFromUtc).Select(x => (decimal?)x.Amount).FirstOrDefaultAsync(ct);
        if (price is null) return Conflict(new { message = "Для товара нет действующей цены" });
        var item = sale.AddItem(productId.Value, quantity, price.Value);
        db.Entry(item).State = EntityState.Added;
        await db.SaveChangesAsync(ct);
        return Ok(new { sale.Id, sale.Total, sale.Items });
    }

    [HttpPost("{id:guid}/checkout"), HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> Checkout(Guid id, CancellationToken ct)
    {
        var sale = await db.Sales.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (sale is null) return NotFound();
        foreach (var g in sale.Items.GroupBy(x => x.ProductId))
        {
            var stock = await db.StockBalances.Where(x => x.WarehouseId == sale.WarehouseId && x.ProductId == g.Key).Select(x => (decimal?)x.Quantity).FirstOrDefaultAsync(ct) ?? 0;
            if (stock < g.Sum(x => x.Quantity)) return Conflict(new { message = "Недостаточно остатка для оплаты", productId = g.Key, available = stock });
        }
        sale.AwaitPayment();
        await db.SaveChangesAsync(ct);
        return Ok(new { sale.Id, sale.Status, sale.Total });
    }

    [HttpPost("{id:guid}/payments"), HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> CreatePayment(Guid id, CreatePaymentRequest r, CancellationToken ct)
    {
        var sale = await db.Sales.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (sale is null) return NotFound();
        if (sale.Status != SaleStatus.AwaitingPayment) return Conflict(new { message = "Сначала переведите чек к оплате" });
        var already = await db.Payments.Where(x => x.SaleId == id && x.Status == PaymentStatus.Paid).SumAsync(x => (decimal?)x.Amount, ct) ?? 0;
        var amount = r.Amount <= 0 ? sale.Total - already : r.Amount;
        if (amount <= 0 || already + amount > sale.Total) return BadRequest(new { message = "Некорректная сумма оплаты" });
        var p = new Payment(sale.Id, amount, r.Method);
        if (r.Method == PaymentMethod.Cash) p.MarkPaid();
        db.Payments.Add(p);
        await db.SaveChangesAsync(ct);
        var paid = already + (p.Status == PaymentStatus.Paid ? p.Amount : 0);
        if (paid == sale.Total)
        {
            sale.MarkPaid();
            await db.SaveChangesAsync(ct);
        }
        return Ok(new { p.Id, p.Amount, p.Method, p.Status, saleStatus = sale.Status, remaining = sale.Total - paid });
    }

    [HttpGet("{id:guid}"), HasPermission(Permissions.SaleCreate)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var s = await db.Sales.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
        return s is null ? NotFound() : Ok(new { s.Id, s.StoreId, s.WarehouseId, s.CashRegisterId, s.CashierId, s.Status, s.Total, s.Items });
    }
}

public sealed record CreateSaleRequest(Guid StoreId, Guid WarehouseId, Guid CashRegisterId);
public sealed record AddBarcodeItemRequest(string Barcode, decimal Quantity);
public sealed record CreatePaymentRequest(PaymentMethod Method, decimal Amount);

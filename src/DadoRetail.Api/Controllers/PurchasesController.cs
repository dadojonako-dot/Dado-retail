using DadoRetail.Api.Security;
using DadoRetail.Application.Security;
using DadoRetail.Domain.Inventory;
using DadoRetail.Domain.Purchasing;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DadoRetail.Api.Controllers;

[ApiController, Route("api/v1/purchases")]
public sealed class PurchasesController(DadoRetailDbContext db) : ControllerBase
{
    [HttpGet, HasPermission(Permissions.ProductView)]
    public async Task<IActionResult> List(CancellationToken ct) => Ok(await db.Purchases.AsNoTracking()
        .Include(x => x.Items)
        .OrderByDescending(x => x.CreatedAtUtc)
        .Take(500)
        .Select(x => new
        {
            x.Id,
            x.SupplierId,
            x.WarehouseId,
            x.DocumentNumber,
            x.Status,
            x.PostedAtUtc,
            Items = x.Items.Count,
            Total = x.Items.Sum(i => i.Quantity * i.PurchasePrice),
            x.CreatedAtUtc
        }).ToListAsync(ct));

    [HttpGet("{id:guid}"), HasPermission(Permissions.ProductView)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var purchase = await db.Purchases.AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (purchase is null) return NotFound();

        var productIds = purchase.Items.Select(x => x.ProductId).Distinct().ToArray();
        var products = await db.Products.AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Sku, x.Name })
            .ToDictionaryAsync(x => x.Id, ct);

        return Ok(new
        {
            purchase.Id,
            purchase.SupplierId,
            purchase.WarehouseId,
            purchase.DocumentNumber,
            purchase.Status,
            purchase.PostedAtUtc,
            purchase.CreatedAtUtc,
            Total = purchase.Items.Sum(x => x.Quantity * x.PurchasePrice),
            Items = purchase.Items.Select(x => new
            {
                x.Id,
                x.ProductId,
                Sku = products.TryGetValue(x.ProductId, out var p) ? p.Sku : string.Empty,
                Name = products.TryGetValue(x.ProductId, out var p2) ? p2.Name : string.Empty,
                x.Quantity,
                x.PurchasePrice,
                x.ExpiryDate,
                Total = x.Quantity * x.PurchasePrice
            })
        });
    }

    [HttpPost, HasPermission(Permissions.PurchaseCreate)]
    public async Task<IActionResult> Create(CreatePurchaseRequest r, CancellationToken ct)
    {
        if (!await db.Set<DadoRetail.Domain.Partners.Supplier>().AnyAsync(x => x.Id == r.SupplierId, ct))
            return BadRequest(new { message = "Поставщик не найден" });
        if (!await db.Warehouses.AnyAsync(x => x.Id == r.WarehouseId, ct))
            return BadRequest(new { message = "Склад не найден" });
        if (string.IsNullOrWhiteSpace(r.DocumentNumber))
            return BadRequest(new { message = "Укажите номер документа" });

        var p = new Purchase(r.SupplierId, r.WarehouseId, r.DocumentNumber);
        db.Purchases.Add(p);
        await db.SaveChangesAsync(ct);
        return Ok(new { p.Id, p.Status });
    }

    [HttpPost("{id:guid}/items/by-barcode"), HasPermission(Permissions.PurchaseCreate)]
    public async Task<IActionResult> AddItem(Guid id, AddPurchaseItemRequest r, CancellationToken ct)
    {
        var p = await db.Purchases.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();
        if (p.Status != DocumentStatus.Draft) return Conflict(new { message = "Проведенный или отмененный документ нельзя редактировать" });
        var productId = await db.Barcodes.Where(x => x.Value == r.Barcode).Select(x => (Guid?)x.ProductId).FirstOrDefaultAsync(ct);
        if (productId is null) return NotFound(new { message = "Товар по штрихкоду не найден" });
        var item = p.AddItem(productId.Value, r.Quantity, r.PurchasePrice, r.ExpiryDate);
        db.Entry(item).State = EntityState.Added;
        await db.SaveChangesAsync(ct);
        return Ok(new { p.Id, items = p.Items.Count });
    }

    [HttpPost("{id:guid}/post"), HasPermission(Permissions.PurchasePost)]
    public async Task<IActionResult> Post(Guid id, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var p = await db.Purchases.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return NotFound();
        if (p.Status != DocumentStatus.Draft) return Conflict(new { message = "Документ уже проведен или отменен" });
        foreach (var i in p.Items)
        {
            var b = await db.StockBalances.FirstOrDefaultAsync(x => x.WarehouseId == p.WarehouseId && x.ProductId == i.ProductId, ct);
            if (b is null)
            {
                b = new StockBalance(p.WarehouseId, i.ProductId);
                db.StockBalances.Add(b);
            }
            b.Apply(i.Quantity);
            db.StockMovements.Add(new StockMovement(p.WarehouseId, i.ProductId, i.Quantity, StockMovementType.Receipt, p.Id));
        }
        p.MarkPosted();
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return Ok(new { p.Id, p.Status, p.PostedAtUtc });
    }
}

public sealed record CreatePurchaseRequest(Guid SupplierId, Guid WarehouseId, string DocumentNumber);
public sealed record AddPurchaseItemRequest(string Barcode, decimal Quantity, decimal PurchasePrice, DateOnly? ExpiryDate);

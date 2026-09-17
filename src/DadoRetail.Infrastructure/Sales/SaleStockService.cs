using DadoRetail.Application.Sales;
using DadoRetail.Domain.Inventory;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DadoRetail.Infrastructure.Sales;

public sealed class SaleStockService(DadoRetailDbContext db) : ISaleStockService
{
    public async Task DeductAsync(Guid warehouseId, Guid saleId, CancellationToken ct = default)
    {
        var sale = await db.Sales.Include(x=>x.Items).FirstAsync(x=>x.Id==saleId, ct);
        foreach (var item in sale.Items)
        {
            var balance = await db.StockBalances.FirstOrDefaultAsync(x=>x.WarehouseId==warehouseId && x.ProductId==item.ProductId, ct)
                ?? throw new InvalidOperationException($"Остаток товара {item.ProductId} не найден");
            if (balance.Quantity < item.Quantity) throw new InvalidOperationException($"Недостаточно товара {item.ProductId}");
            balance.Apply(-item.Quantity);
            db.StockMovements.Add(new StockMovement(warehouseId, item.ProductId, -item.Quantity, StockMovementType.Sale, saleId));
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(Guid warehouseId, Guid saleId, CancellationToken ct = default)
    {
        var sale = await db.Sales.Include(x=>x.Items).FirstAsync(x=>x.Id==saleId, ct);
        foreach (var item in sale.Items)
        {
            var balance = await db.StockBalances.FirstAsync(x=>x.WarehouseId==warehouseId && x.ProductId==item.ProductId, ct);
            balance.Apply(item.Quantity);
            db.StockMovements.Add(new StockMovement(warehouseId, item.ProductId, item.Quantity, StockMovementType.CustomerReturn, saleId));
        }
        await db.SaveChangesAsync(ct);
    }
}

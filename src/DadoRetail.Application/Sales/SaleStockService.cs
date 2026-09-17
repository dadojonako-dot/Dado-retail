using DadoRetail.Domain.Inventory;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DadoRetail.Application.Sales;

public sealed class SaleStockService(DadoRetailDbContext db)
{
    public async Task DeductAsync(Guid warehouseId, Guid saleId, CancellationToken ct = default)
    {
        var sale = await db.Sales.Include(x => x.Items).FirstAsync(x => x.Id == saleId, ct);
        foreach (var item in sale.Items)
        {
            var balance = await db.StockBalances.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.ProductId == item.ProductId, ct)
                ?? throw new InvalidOperationException($"No stock balance for product {item.ProductId}");
            if (balance.Quantity < item.Quantity) throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");
            balance.Apply(-item.Quantity);
            db.StockMovements.Add(new StockMovement(warehouseId, item.ProductId, -item.Quantity, StockMovementType.Sale, saleId));
        }
        await db.SaveChangesAsync(ct);
    }
}

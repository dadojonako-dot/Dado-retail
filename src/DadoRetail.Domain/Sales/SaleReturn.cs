using DadoRetail.Domain.Common;
namespace DadoRetail.Domain.Sales;
public enum SaleReturnStatus{Draft,Fiscalizing,Completed,Failed,Cancelled}
public sealed class SaleReturn:Entity{
 private SaleReturn(){}
 public SaleReturn(Guid saleId,Guid warehouseId,Guid cashierId){SaleId=saleId;WarehouseId=warehouseId;CashierId=cashierId;}
 public Guid SaleId{get;private set;}public Guid WarehouseId{get;private set;}public Guid CashierId{get;private set;}public SaleReturnStatus Status{get;private set;}=SaleReturnStatus.Draft;public ICollection<SaleReturnItem> Items{get;private set;}=new List<SaleReturnItem>();public decimal Total=>Items.Sum(x=>x.Quantity*x.UnitPrice);
 public void AddItem(Guid productId,decimal quantity,decimal unitPrice){if(Status!=SaleReturnStatus.Draft)throw new InvalidOperationException();if(quantity<=0)throw new ArgumentOutOfRangeException(nameof(quantity));Items.Add(new SaleReturnItem(Id,productId,quantity,unitPrice));MarkUpdated();}
 public void BeginFiscalization(){if(Status is not (SaleReturnStatus.Draft or SaleReturnStatus.Failed))throw new InvalidOperationException();Status=SaleReturnStatus.Fiscalizing;MarkUpdated();}public void Fail(){if(Status!=SaleReturnStatus.Fiscalizing)throw new InvalidOperationException();Status=SaleReturnStatus.Failed;MarkUpdated();}public void Complete(){if(Status!=SaleReturnStatus.Fiscalizing)throw new InvalidOperationException();Status=SaleReturnStatus.Completed;MarkUpdated();}}
public sealed class SaleReturnItem:Entity{private SaleReturnItem(){}internal SaleReturnItem(Guid saleReturnId,Guid productId,decimal quantity,decimal unitPrice){SaleReturnId=saleReturnId;ProductId=productId;Quantity=quantity;UnitPrice=unitPrice;}public Guid SaleReturnId{get;private set;}public Guid ProductId{get;private set;}public decimal Quantity{get;private set;}public decimal UnitPrice{get;private set;}}

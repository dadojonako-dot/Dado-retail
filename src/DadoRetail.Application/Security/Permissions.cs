namespace DadoRetail.Application.Security;

public static class Permissions
{
    public const string ProductView = "product.view";
    public const string ProductCreate = "product.create";
    public const string ProductEdit = "product.edit";
    public const string PurchaseCreate = "purchase.create";
    public const string PurchasePost = "purchase.post";
    public const string SaleCreate = "sale.create";
    public const string SaleRefund = "sale.refund";
    public const string InventoryCreate = "inventory.create";
    public const string InventoryApprove = "inventory.approve";
    public const string PriceEdit = "price.edit";
    public const string ReportView = "report.view";
    public const string UserManage = "user.manage";
    public const string FiscalSale = "fiscal.sale";
    public const string FiscalRetry = "fiscal.retry";
    public const string NonFiscalExecute = "non_fiscal.execute";
    public const string NonFiscalApprove = "non_fiscal.approve";
}

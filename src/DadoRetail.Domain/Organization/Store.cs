using DadoRetail.Domain.Common;

namespace DadoRetail.Domain.Organization;

public sealed class Store : Entity
{
    private Store() { }
    public Store(string code, string name) { Code = code.Trim(); Name = name.Trim(); }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
}

public sealed class Warehouse : Entity
{
    private Warehouse() { }
    public Warehouse(Guid storeId, string code, string name) { StoreId = storeId; Code = code.Trim(); Name = name.Trim(); }
    public Guid StoreId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
}

public sealed class CashRegister : Entity
{
    private CashRegister() { }
    public CashRegister(Guid storeId, string code) { StoreId = storeId; Code = code.Trim(); }
    public Guid StoreId { get; private set; }
    public string Code { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
}

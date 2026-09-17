using DadoRetail.Domain.Common;

namespace DadoRetail.Domain.Identity;

public sealed class User : Entity
{
    private User() { }
    public User(string username, string passwordHash)
    {
        Username = username.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
    }
    public string Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;
    public ICollection<UserRole> Roles { get; private set; } = new List<UserRole>();
}

public sealed class Role : Entity
{
    private Role() { }
    public Role(string name) => Name = name.Trim();
    public string Name { get; private set; } = null!;
    public ICollection<UserRole> Users { get; private set; } = new List<UserRole>();
    public ICollection<RolePermission> Permissions { get; private set; } = new List<RolePermission>();
}

public sealed class Permission : Entity
{
    private Permission() { }
    public Permission(string code) => Code = code.Trim().ToLowerInvariant();
    public string Code { get; private set; } = null!;
    public ICollection<RolePermission> Roles { get; private set; } = new List<RolePermission>();
}

public sealed class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}

public sealed class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}

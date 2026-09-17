using Microsoft.AspNetCore.Authorization;

namespace DadoRetail.Api.Security;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Claims.Any(c => c.Type == "permission" && c.Value == requirement.Permission))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) => Policy = $"permission:{permission}";
}

public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        const string prefix = "permission:";
        if (!policyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return await base.GetPolicyAsync(policyName);
        var permission = policyName[prefix.Length..];
        return new AuthorizationPolicyBuilder().RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(permission)).Build();
    }
}

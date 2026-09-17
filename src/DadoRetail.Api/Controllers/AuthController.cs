using DadoRetail.Api.Security;
using DadoRetail.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DadoRetail.Domain.Identity;

namespace DadoRetail.Api.Controllers;

[ApiController, Route("api/v1/auth")]
public sealed class AuthController(DadoRetailDbContext db, JwtTokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var username = request.Username.Trim().ToLowerInvariant();
        var user = await db.Users.Include(x=>x.Roles).ThenInclude(x=>x.Role).ThenInclude(x=>x.Permissions).ThenInclude(x=>x.Permission).FirstOrDefaultAsync(x=>x.Username==username && x.IsActive, ct);
        if (user is null) return Unauthorized();
        var hasher = new PasswordHasher<User>();
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed) return Unauthorized();
        var roles = user.Roles.Select(x=>x.Role.Name).ToArray();
        var permissions = user.Roles.SelectMany(x=>x.Role.Permissions).Select(x=>x.Permission.Code).Distinct().ToArray();
        var token = tokens.Create(user.Id, user.Username, roles, permissions);
        return Ok(new { accessToken = token, tokenType = "Bearer", expiresIn = 28800, user = new { user.Id, user.Username, roles, permissions } });
    }
}

public sealed record LoginRequest(string Username, string Password);

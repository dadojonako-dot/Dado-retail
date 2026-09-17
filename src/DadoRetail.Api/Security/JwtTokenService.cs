using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DadoRetail.Api.Security;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public string Create(Guid userId, string username, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is required.");
        var issuer = configuration["Jwt:Issuer"] ?? "DadoRetail";
        var audience = configuration["Jwt:Audience"] ?? "DadoRetailClients";
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, userId.ToString()), new(ClaimTypes.Name, username) };
        claims.AddRange(roles.Distinct().Select(x => new Claim(ClaimTypes.Role, x)));
        claims.AddRange(permissions.Distinct().Select(x => new Claim("permission", x)));
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddHours(8), signingCredentials: credentials));
    }
}

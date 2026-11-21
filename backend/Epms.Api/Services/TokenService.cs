using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Epms.Api.Dtos;
using Microsoft.IdentityModel.Tokens;

namespace Epms.Api.Services;

public record JwtSettings(SymmetricSecurityKey Key, string Issuer, string Audience, int ExpiryMinutes);

public interface ITokenService
{
    AuthResponse Create(UserDto user);
}

public sealed class TokenService(JwtSettings settings) : ITokenService
{
    public AuthResponse Create(UserDto user)
    {
        List<Claim> claims =
        [
            new("sub", user.Id.ToString()),
            new("email", user.Email),
            new("name", user.Name),
            new("role", user.Role.ToString())
        ];
        if (user.EmployeeId is int employeeId)
            claims.Add(new Claim("employeeId", employeeId.ToString()));

        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims,
            expires: DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes),
            signingCredentials: new SigningCredentials(settings.Key, SecurityAlgorithms.HmacSha256));
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), user, token.ValidTo);
    }
}

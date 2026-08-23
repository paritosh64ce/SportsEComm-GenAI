using Microsoft.AspNetCore.Identity;
using SportsEComm.Api.Models;

namespace SportsEComm.Api.Services;

public interface IAuthService
{
    string HashSecretKey(string secretKey);
    bool VerifySecretKey(string secretKey, string hash);
    string GenerateJwtToken(Customer customer);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string HashSecretKey(string secretKey)
    {
        // Using ASP.NET Core PasswordHasher for secure cryptographic hashing at rest
        return _passwordHasher.HashPassword(null!, secretKey);
    }

    public bool VerifySecretKey(string secretKey, string hash)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, hash, secretKey);
        return result != PasswordVerificationResult.Failed;
    }

    public string GenerateJwtToken(Customer customer)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["Secret"] ?? "SportsECommSuperSecretKeyForJWTAuth2026!@#$";
        var issuer = jwtSettings["Issuer"] ?? "SportsECommApi";
        var audience = jwtSettings["Audience"] ?? "SportsECommClient";
        var expiryMinutes = int.TryParse(jwtSettings["ExpiryMinutes"], out var mins) ? mins : 480;

        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = System.Text.Encoding.ASCII.GetBytes(secretKey);
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, customer.Email),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, customer.Name),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, customer.Role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

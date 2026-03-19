using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UsuariosAPI.Models;

namespace UsuariosAPI.Services
{
    public interface ITokenService
    {
        (string token, DateTime expiration) GenerateAccessToken(CuentaUsuario cuenta);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        public TokenService(IConfiguration config) => _config = config;

        public (string token, DateTime expiration) GenerateAccessToken(CuentaUsuario cuenta)
        {
            var jwt        = _config.GetSection("JwtSettings");
            var key        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
            var creds      = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(int.Parse(jwt["AccessTokenExpiryMinutes"] ?? "60"));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, cuenta.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name,              cuenta.Username),
                new Claim("userId",    cuenta.UsuarioId.ToString()),
                new Claim("accountId", cuenta.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer:             jwt["Issuer"]   ?? "UsuariosAPI",
                audience:           jwt["Audience"] ?? "UsuariosAPIClients",
                claims:             claims,
                notBefore:          DateTime.UtcNow,
                expires:            expiration,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
        }

        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var jwt = _config.GetSection("JwtSettings");
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = jwt["Issuer"]   ?? "UsuariosAPI",
                ValidAudience            = jwt["Audience"] ?? "UsuariosAPIClients",
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!))
            };

            var handler   = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, validationParams, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token inválido.");

            return principal;
        }
    }
}

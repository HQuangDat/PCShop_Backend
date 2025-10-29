using Humanizer;
using Microsoft.IdentityModel.Tokens;
using PCShop_Backend.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PCShop_Backend.Service
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User existUser)
        {
            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, existUser.Username),
                        new Claim(ClaimTypes.Role, existUser.Role.RoleName),
                        new Claim(ClaimTypes.NameIdentifier, existUser.UserId.ToString())
                    };

            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration.");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            Log.Information("Generated JWT token for user: {Username}, Role: {Role}, Expires in: {Expiry}", existUser.Username, existUser.Role.RoleName, expires.Humanize());
            return tokenHandler.WriteToken(token);
        }
    }
}

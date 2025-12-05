using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Taskify.Services.Interface;
using Taskify.Services.Utilities;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Taskify.Services.Implementation
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtConfig _jwtConfig;

        public JwtTokenService(IOptions<JwtConfig> opt)
        {
            _jwtConfig = opt.Value;
        }
   

        public string GetJwtToken(IEnumerable<string> roles, string userId, string username, string securityStamp)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            if (!string.IsNullOrEmpty(securityStamp))
            {
                // include security stamp so tokens can be invalidated when stamp changes
                userClaims.Add(new Claim("secStamp", securityStamp));
            }

            foreach(var role in roles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SigningKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audiences,
                claims: userClaims,
                expires: DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationTime),
                signingCredentials: credentials
             );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}

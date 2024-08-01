using AppBackend.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace AppBackend.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenData ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                if (validatedToken != null && validatedToken is JwtSecurityToken jwtToken)
                {
                    var employeeIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    var tenantIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "TenantID");

                    if (employeeIdClaim != null && tenantIdClaim != null)
                    {
                        return new TokenData
                        {
                            EmployeeId = int.Parse(employeeIdClaim.Value),
                            TenantId = int.Parse(tenantIdClaim.Value)
                        };
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}

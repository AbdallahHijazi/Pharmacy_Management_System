using Microsoft.IdentityModel.Tokens;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Security
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IAuthSettingsProvider _authSettingsProvider;

        public JwtTokenGenerator(IAuthSettingsProvider authSettingsProvider)
        {
            _authSettingsProvider = authSettingsProvider;
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new("branchId", user.BranchId.ToString()),
            new(ClaimTypes.Role, user.Role?.Name ?? string.Empty)
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_authSettingsProvider.Secret));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _authSettingsProvider.Issuer,
                audience: _authSettingsProvider.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_authSettingsProvider.ExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

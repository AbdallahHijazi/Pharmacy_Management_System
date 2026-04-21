using Microsoft.Extensions.Configuration;
using Pharmacy.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Infrastructure.Security
{
    public class AuthSettingsProvider : IAuthSettingsProvider
    {
        private readonly IConfiguration _configuration;

        public AuthSettingsProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Secret => _configuration["Jwt:Secret"]!;
        public string Issuer => _configuration["Jwt:Issuer"]!;
        public string Audience => _configuration["Jwt:Audience"]!;
        public int ExpiryMinutes => int.Parse(_configuration["Jwt:ExpiryMinutes"]!);
    }
}

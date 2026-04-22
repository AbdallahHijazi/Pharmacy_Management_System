using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Settings
{
    public class UpdateSystemSettingRequestDto
    {
        public Guid SettingId { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}

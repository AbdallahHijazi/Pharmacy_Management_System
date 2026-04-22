using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Settings.Commands.UpdateSetting
{
    public class UpdateSettingCommand : IRequest<Unit>
    {
        public Guid SettingId { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}

using MediatR;
using Pharmacy.Application.DTOs.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Settings.Queries.GetSettings
{
    public class GetSettingsQuery : IRequest<List<SystemSettingDto>>
    {
    }
}

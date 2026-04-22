using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Settings;
using Pharmacy.Domain.Entities.Settings;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Settings.Queries.GetSettings
{
    public class GetSettingsQueryHandler : IRequestHandler<GetSettingsQuery, List<SystemSettingDto>>
    {
        private readonly IRepository<SystemSetting> _settingsRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetSettingsQueryHandler(
            IRepository<SystemSetting> settingsRepository,
            ICurrentUserService currentUserService)
        {
            _settingsRepository = settingsRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<SystemSettingDto>> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            var settings = await _settingsRepository
                .GetAll()
                .Where(s => !s.IsDeleted)
                .Select(s => new SystemSettingDto
                {
                    SettingId = s.Id,
                    Key = s.Key,
                    Value = s.Value,
                    Description = s.Description
                })
                .ToListAsync(cancellationToken);

            return settings;
        }
    }
}

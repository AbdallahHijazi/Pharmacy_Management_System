using MediatR;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Domain.Entities.Settings;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Settings.Commands.UpdateSetting
{
    public class UpdateSettingCommandHandler : IRequestHandler<UpdateSettingCommand, Unit>
    {
        private readonly IRepository<SystemSetting> _settingsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateSettingCommandHandler(
            IRepository<SystemSetting> settingsRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _settingsRepository = settingsRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(UpdateSettingCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (string.IsNullOrWhiteSpace(request.Value))
                throw new BadRequestException("القيمة مطلوبة");

            var setting = _settingsRepository.Get(request.SettingId);

            if (setting is null || setting.IsDeleted)
                throw new NotFoundException("SystemSetting", request.SettingId);

            setting.Value = request.Value.Trim();
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedByUserId = _currentUserService.UserId.Value;

            _settingsRepository.Update(setting);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}


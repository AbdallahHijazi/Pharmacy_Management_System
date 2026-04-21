using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Domain.Entities.Identity;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserPasswordHasher _passwordHasher;

        public ChangePasswordCommandHandler(
            IRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IUserPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _passwordHasher = passwordHasher;
        }

        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                throw new BadRequestException("كلمة المرور الحالية مطلوبة");

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new BadRequestException("كلمة المرور الجديدة مطلوبة");

            if (request.CurrentPassword == request.NewPassword)
                throw new BadRequestException("كلمة المرور الجديدة يجب أن تكون مختلفة عن الحالية");

            var user = await _userRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    u => u.Id == _currentUserService.UserId.Value && !u.IsDeleted,
                    cancellationToken);

            if (user is null)
                throw new UnauthorizedException("المستخدم الحالي غير موجود");

            var isValidCurrentPassword = _passwordHasher.Verify(request.CurrentPassword, user.PasswordHash);

            if (!isValidCurrentPassword)
                throw new UnauthorizedException("كلمة المرور الحالية غير صحيحة");

            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

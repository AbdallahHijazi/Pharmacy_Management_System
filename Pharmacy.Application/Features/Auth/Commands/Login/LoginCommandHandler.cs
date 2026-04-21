using MediatR;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Auth;
using Pharmacy.Domain.Entities.Identity;
using Pharmacy.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        private readonly IRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public LoginCommandHandler(
            IRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            IUserPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetAll()
                                            .Include(u => u.Role)
                                            .FirstOrDefaultAsync
                                            (
                                                u => u.Email == request.Email && !u.IsDeleted,
                                                cancellationToken
                                            );

            if (user is null)
                throw new UnauthorizedException("البريد الإلكتروني أو كلمة المرور غير صحيحة");

            var isValidPassword = _passwordHasher.Verify(request.Password, user.PasswordHash);

            if (!isValidPassword)
                throw new UnauthorizedException("البريد الإلكتروني أو كلمة المرور غير صحيحة");

            if (!user.IsActive)
                throw new UnauthorizedException("المستخدم غير مفعل");

            user.LastLoginAt = DateTime.UtcNow;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var token = _jwtTokenGenerator.GenerateToken(user);

            return new LoginResponseDto
            {
                Token = token,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role?.Name ?? string.Empty,
                BranchId = user.BranchId
            };
        }
    }
}

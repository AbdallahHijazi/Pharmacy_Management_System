using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Categories;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDetailsDto>
    {
        private readonly IRepository<ProductCategory> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCategoryCommandHandler(
            IRepository<ProductCategory> categoryRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<CategoryDetailsDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("اسم التصنيف مطلوب");

            var category = await _categoryRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    c => c.Id == request.CategoryId && !c.IsDeleted,
                    cancellationToken);

            if (category is null)
                throw new NotFoundException("Category", request.CategoryId);

            var normalizedName = request.Name.Trim();

            var exists = await _categoryRepository
                .GetAll()
                .AnyAsync(
                    c => c.Id != request.CategoryId &&
                         !c.IsDeleted &&
                         c.Name.ToLower() == normalizedName.ToLower(),
                    cancellationToken);

            if (exists)
                throw new StatusAlreadyExistsException(request.Name);

            category.Name = normalizedName;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedByUserId = _currentUserService.UserId.Value;

            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CategoryDetailsDto
            {
                CategoryId = category.Id,
                Name = category.Name
            };
        }
    }
}

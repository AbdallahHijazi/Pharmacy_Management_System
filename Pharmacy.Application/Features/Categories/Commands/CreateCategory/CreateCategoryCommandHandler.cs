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

namespace Pharmacy.Application.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryListItemDto>
    {
        private readonly IRepository<ProductCategory> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateCategoryCommandHandler(
            IRepository<ProductCategory> categoryRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<CategoryListItemDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BadRequestException("اسم التصنيف مطلوب");

            var normalizedName = request.Name.Trim();

            var exists = await _categoryRepository
                .GetAll()
                .AnyAsync(
                    c => !c.IsDeleted && c.Name.ToLower() == normalizedName.ToLower(),
                    cancellationToken);

            if (exists)
                throw new StatusAlreadyExistsException(request.Name);

            var category = new ProductCategory
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUserService.UserId.Value,
                IsDeleted = false
            };

            _categoryRepository.Add(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CategoryListItemDto
            {
                CategoryId = category.Id,
                Name = category.Name
            };
        }
    }
}

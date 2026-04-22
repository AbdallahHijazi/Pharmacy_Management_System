using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
    {
        private readonly IRepository<ProductCategory> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteCategoryCommandHandler(
            IRepository<ProductCategory> categoryRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            var category = await _categoryRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    c => c.Id == request.CategoryId && !c.IsDeleted,
                    cancellationToken);

            if (category is null)
                throw new NotFoundException("Category", request.CategoryId);

            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedByUserId = _currentUserService.UserId.Value;

            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

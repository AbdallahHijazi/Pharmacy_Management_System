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

namespace Pharmacy.Application.Features.Categories.Queries.GetCategories
{
    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryListItemDto>>
    {
        private readonly IRepository<ProductCategory> _categoryRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetCategoriesQueryHandler(
            IRepository<ProductCategory> categoryRepository,
            ICurrentUserService currentUserService)
        {
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<CategoryListItemDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            var categories = await _categoryRepository
                .GetAll()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryListItemDto
                {
                    CategoryId = c.Id,
                    Name = c.Name
                })
                .ToListAsync(cancellationToken);

            return categories;
        }
    }
}

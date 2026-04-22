using MediatR;
using Pharmacy.Application.DTOs.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommand : IRequest<CategoryListItemDto>
    {
        public string Name { get; set; } = string.Empty;
    }
}

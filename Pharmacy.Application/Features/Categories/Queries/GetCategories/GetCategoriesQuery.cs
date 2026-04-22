using MediatR;
using Pharmacy.Application.DTOs.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Categories.Queries.GetCategories
{
    public class GetCategoriesQuery : IRequest<List<CategoryListItemDto>>
    {
    }
}

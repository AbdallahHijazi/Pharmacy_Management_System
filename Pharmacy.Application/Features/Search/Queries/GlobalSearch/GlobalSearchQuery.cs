using MediatR;
using Pharmacy.Application.DTOs.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Search.Queries.GlobalSearch
{
    public class GlobalSearchQuery : IRequest<List<GlobalSearchResultDto>>
    {
        public string Query { get; set; } = string.Empty;
    }
}

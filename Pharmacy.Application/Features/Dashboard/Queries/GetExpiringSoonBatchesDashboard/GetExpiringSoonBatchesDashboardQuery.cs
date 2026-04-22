using MediatR;
using Pharmacy.Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Dashboard.Queries.GetExpiringSoonBatchesDashboard
{
    public class GetExpiringSoonBatchesDashboardQuery : IRequest<List<ExpiringSoonBatchDto>>
    {
    }
}

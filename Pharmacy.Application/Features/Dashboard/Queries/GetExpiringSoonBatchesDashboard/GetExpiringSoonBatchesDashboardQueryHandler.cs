using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Dashboard;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Dashboard.Queries.GetExpiringSoonBatchesDashboard
{
    public class GetExpiringSoonBatchesDashboardQueryHandler : IRequestHandler<GetExpiringSoonBatchesDashboardQuery, List<ExpiringSoonBatchDto>>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetExpiringSoonBatchesDashboardQueryHandler(
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<ExpiringSoonBatchDto>> Handle(GetExpiringSoonBatchesDashboardQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var branchId = _currentUserService.BranchId.Value;
            var today = DateTime.UtcNow.Date;
            var maxDate = today.AddDays(30);

            var batches = await _stockBatchRepository
                .GetAllAsNoTracking()
                .Include(sb => sb.Product)
                .Where(sb => !sb.IsDeleted &&
                             sb.BranchId == branchId &&
                             sb.AvailableQuantity > 0 &&
                             sb.ExpiryDate.Date > today &&
                             sb.ExpiryDate.Date <= maxDate)
                .OrderBy(sb => sb.ExpiryDate)
                .ThenBy(sb => sb.Product.Name)
                .Select(sb => new ExpiringSoonBatchDto
                {
                    StockBatchId = sb.Id,
                    ProductId = sb.ProductId,
                    ProductName = sb.Product.Name,
                    BatchNumber = sb.BatchNumber,
                    ExpiryDate = sb.ExpiryDate,
                    AvailableQuantity = sb.AvailableQuantity
                })
                .ToListAsync(cancellationToken);

            return batches;
        }
    }
}

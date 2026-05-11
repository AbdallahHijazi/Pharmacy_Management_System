using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Reports;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;

namespace Pharmacy.Application.Features.Reports.Queries.GetStockConsistencyDiagnostics
{
    public sealed class GetStockConsistencyDiagnosticsQueryHandler
        : IRequestHandler<GetStockConsistencyDiagnosticsQuery, StockConsistencyDiagnosticsDto>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetStockConsistencyDiagnosticsQueryHandler(
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<StockConsistencyDiagnosticsDto> Handle(
            GetStockConsistencyDiagnosticsQuery request,
            CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var branchId = _currentUserService.BranchId.Value;

            var batches = await _stockBatchRepository
                .GetAllAsNoTracking()
                .Include(sb => sb.Product)
                .Where(sb => !sb.IsDeleted && sb.BranchId == branchId)
                .ToListAsync(cancellationToken);

            var batchIssues = new List<StockBatchConsistencyIssueDto>();

            foreach (var sb in batches)
            {
                var productName = sb.Product?.Name ?? string.Empty;

                if (sb.AvailableQuantity < 0)
                {
                    batchIssues.Add(new StockBatchConsistencyIssueDto
                    {
                        StockBatchId = sb.Id,
                        ProductId = sb.ProductId,
                        ProductName = productName,
                        BatchNumber = sb.BatchNumber,
                        IssueCode = "NEGATIVE_AVAILABLE",
                        Message = "الكمية المتاحة سالبة لهذه الدفعة.",
                        AvailableQuantity = sb.AvailableQuantity,
                        ReceivedQuantity = sb.ReceivedQuantity
                    });
                }

                if (sb.AvailableQuantity > sb.ReceivedQuantity)
                {
                    batchIssues.Add(new StockBatchConsistencyIssueDto
                    {
                        StockBatchId = sb.Id,
                        ProductId = sb.ProductId,
                        ProductName = productName,
                        BatchNumber = sb.BatchNumber,
                        IssueCode = "AVAILABLE_EXCEEDS_RECEIVED",
                        Message = "الكمية المتاحة أكبر من الكمية المستلمة لهذه الدفعة.",
                        AvailableQuantity = sb.AvailableQuantity,
                        ReceivedQuantity = sb.ReceivedQuantity
                    });
                }
            }

            var productIssues = new List<ProductStockConsistencyIssueDto>();

            foreach (var g in batches.GroupBy(b => b.ProductId))
            {
                var totalAvailable = g.Sum(x => x.AvailableQuantity);
                var totalReceived = g.Sum(x => x.ReceivedQuantity);
                var productName = g.First().Product?.Name ?? string.Empty;

                if (totalAvailable < 0)
                {
                    productIssues.Add(new ProductStockConsistencyIssueDto
                    {
                        ProductId = g.Key,
                        ProductName = productName,
                        IssueCode = "NEGATIVE_TOTAL_AVAILABLE",
                        Message = "مجموع الكميات المتاحة على مستوى المنتج سالب (مجموع الدفعات في الفرع).",
                        TotalAvailableQuantity = totalAvailable,
                        TotalReceivedQuantity = totalReceived,
                        BatchCount = g.Count()
                    });
                }
                else if (totalAvailable > totalReceived)
                {
                    productIssues.Add(new ProductStockConsistencyIssueDto
                    {
                        ProductId = g.Key,
                        ProductName = productName,
                        IssueCode = "TOTAL_AVAILABLE_EXCEEDS_TOTAL_RECEIVED",
                        Message =
                            "مجموع الكميات المتاحة للمنتج أكبر من مجموع الكميات المستلمة عبر الدفعات (تعارض على مستوى المنتج).",
                        TotalAvailableQuantity = totalAvailable,
                        TotalReceivedQuantity = totalReceived,
                        BatchCount = g.Count()
                    });
                }
            }

            return new StockConsistencyDiagnosticsDto
            {
                HasIssues = batchIssues.Count > 0 || productIssues.Count > 0,
                BatchIssues = batchIssues,
                ProductIssues = productIssues
            };
        }
    }
}

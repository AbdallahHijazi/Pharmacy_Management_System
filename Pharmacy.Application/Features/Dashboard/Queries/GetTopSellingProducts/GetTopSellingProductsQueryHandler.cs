using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Dashboard;
using Pharmacy.Domain.Entities.Sales;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Dashboard.Queries.GetTopSellingProducts
{
    public class GetTopSellingProductsQueryHandler : IRequestHandler<GetTopSellingProductsQuery, List<TopSellingProductDto>>
    {
        private readonly IRepository<SalesInvoiceItem> _salesInvoiceItemRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetTopSellingProductsQueryHandler(
            IRepository<SalesInvoiceItem> salesInvoiceItemRepository,
            ICurrentUserService currentUserService)
        {
            _salesInvoiceItemRepository = salesInvoiceItemRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<TopSellingProductDto>> Handle(GetTopSellingProductsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var branchId = _currentUserService.BranchId.Value;

            var products = await _salesInvoiceItemRepository
                .GetAll()
                .Include(sii => sii.StockBatch)
                .ThenInclude(sb => sb.Product)
                .Include(sii => sii.SalesInvoice)
                .Where(sii => !sii.IsDeleted &&
                              sii.SalesInvoice.BranchId == branchId &&
                              !sii.SalesInvoice.IsDeleted)
                .GroupBy(sii => new
                {
                    ProductId = sii.StockBatch.ProductId,
                    ProductName = sii.StockBatch.Product.Name
                })
                .Select(g => new TopSellingProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalSoldQuantity = g.Sum(x => x.Quantity),
                    TotalSalesAmount = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.TotalSoldQuantity)
                .ThenByDescending(x => x.TotalSalesAmount)
                .Take(10)
                .ToListAsync(cancellationToken);

            return products;
        }
    }
}

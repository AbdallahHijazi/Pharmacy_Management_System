using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Dashboard;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Entities.Sales;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Dashboard.Queries.GetDashboardStats
{
    public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetDashboardStatsQueryHandler(
            IRepository<Product> productRepository,
            IRepository<Customer> customerRepository,
            IRepository<Supplier> supplierRepository,
            IRepository<SalesInvoice> salesInvoiceRepository,
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _supplierRepository = supplierRepository;
            _salesInvoiceRepository = salesInvoiceRepository;
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var branchId = _currentUserService.BranchId.Value;
            var today = DateTime.UtcNow.Date;
            var nextDay = today.AddDays(1);
            var expiringDate = DateTime.UtcNow.Date.AddDays(30);

            var totalProducts = await _productRepository
                .GetAllAsNoTracking()
                .CountAsync(p => !p.IsDeleted && p.BranchId == branchId, cancellationToken);

            var totalCustomers = await _customerRepository
                .GetAllAsNoTracking()
                .CountAsync(c => !c.IsDeleted && c.BranchId == branchId, cancellationToken);

            var totalSuppliers = await _supplierRepository
                .GetAllAsNoTracking()
                .CountAsync(s => !s.IsDeleted && s.BranchId == branchId, cancellationToken);

            var todayInvoicesCount = await _salesInvoiceRepository
                .GetAllAsNoTracking()
                .CountAsync(
                    si => !si.IsDeleted &&
                          si.BranchId == branchId &&
                          si.CreatedAt >= today &&
                          si.CreatedAt < nextDay,
                    cancellationToken);

            var todaySalesTotal = await _salesInvoiceRepository
                .GetAllAsNoTracking()
                .Where(si => !si.IsDeleted &&
                             si.BranchId == branchId &&
                             si.CreatedAt >= today &&
                             si.CreatedAt < nextDay)
                .Select(si => (decimal?)si.GrandTotal)
                .SumAsync(cancellationToken) ?? 0m;

            var lowStockProductsCount = await _stockBatchRepository
                .GetAllAsNoTracking()
                .Where(sb => !sb.IsDeleted && sb.BranchId == branchId)
                .GroupBy(sb => sb.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalAvailable = g.Sum(x => x.AvailableQuantity)
                })
                .CountAsync(x => x.TotalAvailable > 0 && x.TotalAvailable <= 10, cancellationToken);

            var expiringSoonBatchesCount = await _stockBatchRepository
                .GetAllAsNoTracking()
                .CountAsync(
                    sb => !sb.IsDeleted &&
                          sb.BranchId == branchId &&
                          sb.AvailableQuantity > 0 &&
                          sb.ExpiryDate.Date <= expiringDate,
                    cancellationToken);

            return new DashboardStatsDto
            {
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                TotalSuppliers = totalSuppliers,
                TodayInvoicesCount = todayInvoicesCount,
                TodaySalesTotal = todaySalesTotal,
                LowStockProductsCount = lowStockProductsCount,
                ExpiringSoonBatchesCount = expiringSoonBatchesCount
            };
        }
    }
}

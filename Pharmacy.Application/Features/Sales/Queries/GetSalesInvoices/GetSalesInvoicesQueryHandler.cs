using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.Common.Models;
using Pharmacy.Application.DTOs.Sales;
using Pharmacy.Domain.Entities.Sales;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Sales.Queries.GetSalesInvoices
{
    public class GetSalesInvoicesQueryHandler : IRequestHandler<GetSalesInvoicesQuery, PagedResult<SalesInvoiceListItemDto>>
    {
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetSalesInvoicesQueryHandler(
            IRepository<SalesInvoice> salesInvoiceRepository,
            ICurrentUserService currentUserService)
        {
            _salesInvoiceRepository = salesInvoiceRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<SalesInvoiceListItemDto>> Handle(GetSalesInvoicesQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            if (request.PageNumber <= 0)
                throw new BadRequestException("رقم الصفحة يجب أن يكون أكبر من صفر");

            if (request.PageSize <= 0 || request.PageSize > 100)
                throw new BadRequestException("حجم الصفحة يجب أن يكون بين 1 و 100");

            var branchId = _currentUserService.BranchId.Value;

            var query = _salesInvoiceRepository
                .GetAllAsNoTracking()
                .Include(si => si.Customer)
                .Include(si => si.User)
                .Where(si => !si.IsDeleted && si.BranchId == branchId);

            query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
            {
                ("invoicenumber", "asc") => query.OrderBy(si => si.InvoiceNumber),
                ("grandtotal", "asc") => query.OrderBy(si => si.GrandTotal),
                ("paidamount", "asc") => query.OrderBy(si => si.PaidAmount),
                ("remainingamount", "asc") => query.OrderBy(si => si.RemainingAmount),
                ("createdat", "asc") => query.OrderBy(si => si.CreatedAt),

                ("invoicenumber", _) => query.OrderByDescending(si => si.InvoiceNumber),
                ("grandtotal", _) => query.OrderByDescending(si => si.GrandTotal),
                ("paidamount", _) => query.OrderByDescending(si => si.PaidAmount),
                ("remainingamount", _) => query.OrderByDescending(si => si.RemainingAmount),

                _ => query.OrderByDescending(si => si.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(si => new SalesInvoiceListItemDto
                {
                    SalesInvoiceId = si.Id,
                    InvoiceNumber = si.InvoiceNumber,
                    CustomerId = si.CustomerId,
                    CustomerName = si.Customer != null ? si.Customer.FullName : string.Empty,
                    UserId = si.UserId,
                    UserFullName = si.User.FullName,
                    BranchId = si.BranchId,
                    Subtotal = si.Subtotal,
                    DiscountAmount = si.DiscountAmount,
                    TaxAmount = si.TaxAmount,
                    GrandTotal = si.GrandTotal,
                    PaidAmount = si.PaidAmount,
                    RemainingAmount = si.RemainingAmount,
                    PaymentMethod = si.PaymentMethod.ToString(),
                    Status = si.Status.ToString(),
                    CreatedAt = si.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<SalesInvoiceListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}

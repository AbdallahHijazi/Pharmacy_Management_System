using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.Common.Models;
using Pharmacy.Application.DTOs.Purchases;
using Pharmacy.Domain.Entities.Purchases;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Purchases.Queries.GetPurchaseInvoices
{
    public class GetPurchaseInvoicesQueryHandler : IRequestHandler<GetPurchaseInvoicesQuery, PagedResult<PurchaseInvoiceListItemDto>>
    {
        private readonly IRepository<PurchaseInvoice> _purchaseInvoiceRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetPurchaseInvoicesQueryHandler(
            IRepository<PurchaseInvoice> purchaseInvoiceRepository,
            ICurrentUserService currentUserService)
        {
            _purchaseInvoiceRepository = purchaseInvoiceRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<PurchaseInvoiceListItemDto>> Handle(GetPurchaseInvoicesQuery request, CancellationToken cancellationToken)
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

            var query = _purchaseInvoiceRepository
                .GetAllAsNoTracking()
                .Include(pi => pi.Supplier)
                .Include(pi => pi.User)
                .Where(pi => !pi.IsDeleted && pi.BranchId == branchId);

            query = (request.SortBy?.ToLower(), request.SortDirection?.ToLower()) switch
            {
                ("invoicenumber", "asc") => query.OrderBy(pi => pi.InvoiceNumber),
                ("grandtotal", "asc") => query.OrderBy(pi => pi.GrandTotal),
                ("paidamount", "asc") => query.OrderBy(pi => pi.PaidAmount),
                ("remainingamount", "asc") => query.OrderBy(pi => pi.RemainingAmount),
                ("createdat", "asc") => query.OrderBy(pi => pi.CreatedAt),

                ("invoicenumber", _) => query.OrderByDescending(pi => pi.InvoiceNumber),
                ("grandtotal", _) => query.OrderByDescending(pi => pi.GrandTotal),
                ("paidamount", _) => query.OrderByDescending(pi => pi.PaidAmount),
                ("remainingamount", _) => query.OrderByDescending(pi => pi.RemainingAmount),

                _ => query.OrderByDescending(pi => pi.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(pi => new PurchaseInvoiceListItemDto
                {
                    PurchaseInvoiceId = pi.Id,
                    InvoiceNumber = pi.InvoiceNumber,
                    SupplierId = pi.SupplierId,
                    SupplierName = pi.Supplier.Name,
                    UserId = pi.UserId,
                    UserFullName = pi.User.FullName,
                    BranchId = pi.BranchId,
                    Subtotal = pi.Subtotal,
                    TaxRate = pi.TaxRate,
                    TaxAmount = pi.TaxAmount,
                    GrandTotal = pi.GrandTotal,
                    PaidAmount = pi.PaidAmount,
                    RemainingAmount = pi.RemainingAmount,
                    PaymentMethod = pi.PaymentMethod.ToString(),
                    Status = pi.Status.ToString(),
                    CreatedAt = pi.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<PurchaseInvoiceListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}

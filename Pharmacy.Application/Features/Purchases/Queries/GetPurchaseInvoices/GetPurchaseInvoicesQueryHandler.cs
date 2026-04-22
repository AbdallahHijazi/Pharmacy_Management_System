using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
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
    public class GetPurchaseInvoicesQueryHandler : IRequestHandler<GetPurchaseInvoicesQuery, List<PurchaseInvoiceListItemDto>>
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

        public async Task<List<PurchaseInvoiceListItemDto>> Handle(GetPurchaseInvoicesQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var invoices = await _purchaseInvoiceRepository
                .GetAll()
                .Include(pi => pi.Supplier)
                .Include(pi => pi.User)
                .Where(pi => !pi.IsDeleted && pi.BranchId == _currentUserService.BranchId.Value)
                .OrderByDescending(pi => pi.CreatedAt)
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

            return invoices;
        }
    }
}

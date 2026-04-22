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

namespace Pharmacy.Application.Features.Purchases.Queries.GetPurchaseInvoiceById
{
    public class GetPurchaseInvoiceByIdQueryHandler : IRequestHandler<GetPurchaseInvoiceByIdQuery, PurchaseInvoiceDetailsDto>
    {
        private readonly IRepository<PurchaseInvoice> _purchaseInvoiceRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetPurchaseInvoiceByIdQueryHandler(
            IRepository<PurchaseInvoice> purchaseInvoiceRepository,
            ICurrentUserService currentUserService)
        {
            _purchaseInvoiceRepository = purchaseInvoiceRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PurchaseInvoiceDetailsDto> Handle(GetPurchaseInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var invoice = await _purchaseInvoiceRepository
                .GetAll()
                .Include(pi => pi.Supplier)
                //.Include(pi => pi.PurchaseInvoiceItems)
                .FirstOrDefaultAsync(
                    pi => pi.Id == request.PurchaseInvoiceId &&
                          !pi.IsDeleted &&
                          pi.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (invoice is null)
                throw new NotFoundException("PurchaseInvoice", request.PurchaseInvoiceId);

            return new PurchaseInvoiceDetailsDto
            {
                PurchaseInvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                SupplierId = invoice.SupplierId,
                SupplierName = invoice.Supplier.Name,
                UserId = invoice.UserId,
                BranchId = invoice.BranchId,
                Subtotal = invoice.Subtotal,
                TaxRate = invoice.TaxRate,
                TaxAmount = invoice.TaxAmount,
                GrandTotal = invoice.GrandTotal,
                PaidAmount = invoice.PaidAmount,
                RemainingAmount = invoice.RemainingAmount,
                PaymentMethod = invoice.PaymentMethod.ToString(),
                Status = invoice.Status.ToString(),
                CreatedAt = invoice.CreatedAt
            };
        }
    }
}

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

namespace Pharmacy.Application.Features.Purchases.Queries.GetPurchaseReturns
{
    public class GetPurchaseReturnsQueryHandler : IRequestHandler<GetPurchaseReturnsQuery, List<PurchaseReturnListItemDto>>
    {
        private readonly IRepository<PurchaseReturn> _purchaseReturnRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetPurchaseReturnsQueryHandler(
            IRepository<PurchaseReturn> purchaseReturnRepository,
            ICurrentUserService currentUserService)
        {
            _purchaseReturnRepository = purchaseReturnRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<PurchaseReturnListItemDto>> Handle(GetPurchaseReturnsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var returns = await _purchaseReturnRepository
                .GetAll()
                .Include(pr => pr.PurchaseInvoice)
                .Include(pr => pr.User)
                .Where(pr => !pr.IsDeleted && pr.PurchaseInvoice.BranchId == _currentUserService.BranchId.Value)
                .OrderByDescending(pr => pr.CreatedAt)
                .Select(pr => new PurchaseReturnListItemDto
                {
                    PurchaseReturnId = pr.Id,
                    PurchaseInvoiceId = pr.PurchaseInvoiceId,
                    InvoiceNumber = pr.PurchaseInvoice.InvoiceNumber,
                    UserId = pr.UserId,
                    UserFullName = pr.User.FullName,
                    RefundAmount = pr.RefundAmount,
                    Reason = pr.Reason,
                    CreatedAt = pr.CreatedAt,
                    BranchId = pr.PurchaseInvoice.BranchId
                })
                .ToListAsync(cancellationToken);

            return returns;
        }
    }
}

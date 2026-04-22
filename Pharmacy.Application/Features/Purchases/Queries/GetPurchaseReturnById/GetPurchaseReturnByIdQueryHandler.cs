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

namespace Pharmacy.Application.Features.Purchases.Queries.GetPurchaseReturnById
{
    public class GetPurchaseReturnByIdQueryHandler : IRequestHandler<GetPurchaseReturnByIdQuery, PurchaseReturnDetailsDto>
    {
        private readonly IRepository<PurchaseReturn> _purchaseReturnRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetPurchaseReturnByIdQueryHandler(
            IRepository<PurchaseReturn> purchaseReturnRepository,
            ICurrentUserService currentUserService)
        {
            _purchaseReturnRepository = purchaseReturnRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PurchaseReturnDetailsDto> Handle(GetPurchaseReturnByIdQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var purchaseReturn = await _purchaseReturnRepository
                .GetAll()
                .Include(pr => pr.PurchaseInvoice)
                .FirstOrDefaultAsync(
                    pr => pr.Id == request.PurchaseReturnId &&
                          !pr.IsDeleted &&
                          pr.PurchaseInvoice.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (purchaseReturn is null)
                throw new NotFoundException("PurchaseReturn", request.PurchaseReturnId);

            return new PurchaseReturnDetailsDto
            {
                PurchaseReturnId = purchaseReturn.Id,
                PurchaseInvoiceId = purchaseReturn.PurchaseInvoiceId,
                InvoiceNumber = purchaseReturn.PurchaseInvoice.InvoiceNumber,
                UserId = purchaseReturn.UserId,
                RefundAmount = purchaseReturn.RefundAmount,
                Reason = purchaseReturn.Reason,
                CreatedAt = purchaseReturn.CreatedAt,
                BranchId = purchaseReturn.PurchaseInvoice.BranchId
            };
        }
    }
}

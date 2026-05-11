using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Inventory;
using Pharmacy.Domain.Entities.Inventory;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Queries.GetInventoryTransactions
{
    public class GetInventoryTransactionsQueryHandler : IRequestHandler<GetInventoryTransactionsQuery, List<InventoryTransactionListItemDto>>
    {
        private readonly IRepository<InventoryTransaction> _inventoryTransactionRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetInventoryTransactionsQueryHandler(
            IRepository<InventoryTransaction> inventoryTransactionRepository,
            ICurrentUserService currentUserService)
        {
            _inventoryTransactionRepository = inventoryTransactionRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<InventoryTransactionListItemDto>> Handle(GetInventoryTransactionsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var transactions = await _inventoryTransactionRepository
                .GetAll()
                .Include(t => t.StockBatch)
                .ThenInclude(sb => sb.Product)
                .Include(t => t.User)
                .Where(t => !t.IsDeleted && t.BranchId == _currentUserService.BranchId.Value)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new InventoryTransactionListItemDto
                {
                    InventoryTransactionId = t.Id,
                    StockBatchId = t.StockBatchId,
                    ProductId = t.ProductId ?? t.StockBatch.ProductId,
                    ProductName = t.StockBatch.Product.Name,
                    BatchNumber = t.StockBatch.BatchNumber,
                    Type = t.Type.ToString(),
                    Quantity = t.Quantity,
                    Reason = t.Reason,
                    ReferenceId = t.ReferenceId,
                    ReferenceType = t.ReferenceType.ToString(),
                    UserId = t.UserId,
                    UserFullName = t.User.FullName,
                    BranchId = t.BranchId,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return transactions;
        }
    }
}

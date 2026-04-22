using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Inventory;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Inventory.Queries.GetStockBatchById
{
    public class GetStockBatchByIdQueryHandler : IRequestHandler<GetStockBatchByIdQuery, StockBatchDetailsDto>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetStockBatchByIdQueryHandler(
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<StockBatchDetailsDto> Handle(GetStockBatchByIdQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var stockBatch = await _stockBatchRepository
                .GetAll()
                .Include(sb => sb.Product)
                .Include(sb => sb.Supplier)
                .FirstOrDefaultAsync(
                    sb => sb.Id == request.StockBatchId &&
                          !sb.IsDeleted &&
                          sb.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (stockBatch is null)
                throw new NotFoundException("StockBatch", request.StockBatchId);

            return new StockBatchDetailsDto
            {
                StockBatchId = stockBatch.Id,
                ProductId = stockBatch.ProductId,
                ProductName = stockBatch.Product.Name,
                BatchNumber = stockBatch.BatchNumber,
                ExpiryDate = stockBatch.ExpiryDate,
                PurchasePrice = stockBatch.PurchasePrice,
                ReceivedQuantity = stockBatch.ReceivedQuantity,
                AvailableQuantity = stockBatch.AvailableQuantity,
                SupplierId = stockBatch.SupplierId,
                SupplierName = stockBatch.Supplier.Name,
                BranchId = stockBatch.BranchId
            };
        }
    }
}

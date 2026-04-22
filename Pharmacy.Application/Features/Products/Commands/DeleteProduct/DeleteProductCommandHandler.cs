using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteProductCommandHandler(
            IRepository<Product> productRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم الحالي");

            var product = await _productRepository
                .GetAll()
                .FirstOrDefaultAsync(
                    p => p.Id == request.ProductId &&
                         !p.IsDeleted &&
                         p.BranchId == _currentUserService.BranchId.Value,
                    cancellationToken);

            if (product is null)
                throw new NotFoundException("Product", request.ProductId);

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedByUserId = _currentUserService.UserId.Value;

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}

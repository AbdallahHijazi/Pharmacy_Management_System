using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Search;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Entities.Partners;
using Pharmacy.Domain.Entities.Purchases;
using Pharmacy.Domain.Entities.Sales;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Search.Queries.GlobalSearch
{
    public class GlobalSearchQueryHandler : IRequestHandler<GlobalSearchQuery, List<GlobalSearchResultDto>>
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Supplier> _supplierRepository;
        private readonly IRepository<SalesInvoice> _salesInvoiceRepository;
        private readonly IRepository<PurchaseInvoice> _purchaseInvoiceRepository;
        private readonly ICurrentUserService _currentUserService;

        public GlobalSearchQueryHandler(
                IRepository<Product> productRepository,
                IRepository<Customer> customerRepository,
                IRepository<Supplier> supplierRepository,
                IRepository<SalesInvoice> salesInvoiceRepository,
                IRepository<PurchaseInvoice> purchaseInvoiceRepository,
                ICurrentUserService currentUserService)
        {
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _supplierRepository = supplierRepository;
            _salesInvoiceRepository = salesInvoiceRepository;
            _purchaseInvoiceRepository = purchaseInvoiceRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<GlobalSearchResultDto>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var query = request.Query?.Trim();

            if (string.IsNullOrWhiteSpace(query))
                return new List<GlobalSearchResultDto>();

            var branchId = _currentUserService.BranchId.Value;
            var normalizedQuery = query.ToLower();

            var products = await _productRepository
                .GetAll()
                .Where(p => !p.IsDeleted &&
                            p.BranchId == branchId &&
                            (p.Name.ToLower().Contains(normalizedQuery) ||
                             p.ScientificName.ToLower().Contains(normalizedQuery) ||
                             p.Barcode.ToLower().Contains(normalizedQuery)))
                .OrderBy(p => p.Name)
                .Take(5)
                .Select(p => new GlobalSearchResultDto
                {
                    Type = "Product",
                    Id = p.Id,
                    Title = p.Name,
                    Subtitle = p.Barcode
                })
                .ToListAsync(cancellationToken);

            var customers = await _customerRepository
                .GetAll()
                .Where(c => !c.IsDeleted &&
                            c.BranchId == branchId &&
                            (c.FullName.ToLower().Contains(normalizedQuery) ||
                             c.Phone.ToLower().Contains(normalizedQuery)))
                .OrderBy(c => c.FullName)
                .Take(5)
                .Select(c => new GlobalSearchResultDto
                {
                    Type = "Customer",
                    Id = c.Id,
                    Title = c.FullName,
                    Subtitle = c.Phone
                })
                .ToListAsync(cancellationToken);

            var suppliers = await _supplierRepository
                .GetAll()
                .Where(s => !s.IsDeleted &&
                            s.BranchId == branchId &&
                            (s.Name.ToLower().Contains(normalizedQuery) ||
                             s.ContactPerson.ToLower().Contains(normalizedQuery) ||
                             s.Phone.ToLower().Contains(normalizedQuery)))
                .OrderBy(s => s.Name)
                .Take(5)
                .Select(s => new GlobalSearchResultDto
                {
                    Type = "Supplier",
                    Id = s.Id,
                    Title = s.Name,
                    Subtitle = s.Phone
                })
                .ToListAsync(cancellationToken);
            var salesInvoices = await _salesInvoiceRepository
                .GetAll()
                .Where(si => !si.IsDeleted &&
                             si.BranchId == branchId &&
                             si.InvoiceNumber.ToLower().Contains(normalizedQuery))
                .OrderByDescending(si => si.CreatedAt)
                .Take(5)
                .Select(si => new GlobalSearchResultDto
                {
                    Type = "SalesInvoice",
                    Id = si.Id,
                    Title = si.InvoiceNumber,
                    Subtitle = $"إجمالي: {si.GrandTotal}"
                })
                .ToListAsync(cancellationToken);
            var purchaseInvoices = await _purchaseInvoiceRepository
                .GetAll()
                .Where(pi => !pi.IsDeleted &&
                             pi.BranchId == branchId &&
                             pi.InvoiceNumber.ToLower().Contains(normalizedQuery))
                .OrderByDescending(pi => pi.CreatedAt)
                .Take(5)
                .Select(pi => new GlobalSearchResultDto
                {
                    Type = "PurchaseInvoice",
                    Id = pi.Id,
                    Title = pi.InvoiceNumber,
                    Subtitle = $"إجمالي: {pi.GrandTotal}"
                })
                .ToListAsync(cancellationToken);
            return products
                .Concat(customers)
                .Concat(suppliers)
                .Concat(salesInvoices)
                .Concat(purchaseInvoices)
                .ToList();
        }
    }
}

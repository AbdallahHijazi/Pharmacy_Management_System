using MediatR;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Application.Common.Interfaces;
using Pharmacy.Application.DTOs.Notifications;
using Pharmacy.Domain.Entities.Catalog;
using Pharmacy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Notifications.Queries.GetNotifications
{
    public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, List<NotificationDto>>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetNotificationsQueryHandler(
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUserService.UserId is null)
                throw new UnauthorizedException("المستخدم غير مسجل الدخول");

            if (_currentUserService.BranchId is null)
                throw new UnauthorizedException("لا يوجد فرع مرتبط بالمستخدم");

            var branchId = _currentUserService.BranchId.Value;
            var today = DateTime.UtcNow.Date;
            var expiringSoonDate = today.AddDays(30);

            var batches = await _stockBatchRepository
                .GetAll()
                .Include(sb => sb.Product)
                .Where(sb => !sb.IsDeleted &&
                             sb.BranchId == branchId &&
                             sb.AvailableQuantity > 0)
                .OrderBy(sb => sb.ExpiryDate)
                .ToListAsync(cancellationToken);

            var notifications = new List<NotificationDto>();

            foreach (var batch in batches)
            {
                if (batch.ExpiryDate.Date <= today)
                {
                    notifications.Add(new NotificationDto
                    {
                        Type = "Expired",
                        Title = "دفعة منتهية الصلاحية",
                        Message = $"المنتج {batch.Product.Name} - التشغيلة {batch.BatchNumber} منتهية الصلاحية",
                        ReferenceId = batch.Id,
                        CreatedAt = batch.ExpiryDate
                    });

                    continue;
                }

                if (batch.ExpiryDate.Date <= expiringSoonDate)
                {
                    notifications.Add(new NotificationDto
                    {
                        Type = "ExpiringSoon",
                        Title = "دفعة قريبة من الانتهاء",
                        Message = $"المنتج {batch.Product.Name} - التشغيلة {batch.BatchNumber} تنتهي بتاريخ {batch.ExpiryDate:yyyy-MM-dd}",
                        ReferenceId = batch.Id,
                        CreatedAt = batch.ExpiryDate
                    });
                }

                if (batch.AvailableQuantity <= 10)
                {
                    notifications.Add(new NotificationDto
                    {
                        Type = "LowStock",
                        Title = "مخزون منخفض",
                        Message = $"المنتج {batch.Product.Name} - الكمية المتاحة {batch.AvailableQuantity}",
                        ReferenceId = batch.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }
    }
}

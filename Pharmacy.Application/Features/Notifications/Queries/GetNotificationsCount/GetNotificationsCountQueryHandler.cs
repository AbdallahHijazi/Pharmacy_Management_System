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

namespace Pharmacy.Application.Features.Notifications.Queries.GetNotificationsCount
{
    public class GetNotificationsCountQueryHandler : IRequestHandler<GetNotificationsCountQuery, NotificationsCountDto>
    {
        private readonly IRepository<StockBatch> _stockBatchRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetNotificationsCountQueryHandler(
            IRepository<StockBatch> stockBatchRepository,
            ICurrentUserService currentUserService)
        {
            _stockBatchRepository = stockBatchRepository;
            _currentUserService = currentUserService;
        }

        public async Task<NotificationsCountDto> Handle(GetNotificationsCountQuery request, CancellationToken cancellationToken)
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
                .Where(sb => !sb.IsDeleted &&
                             sb.BranchId == branchId &&
                             sb.AvailableQuantity > 0)
                .Select(sb => new
                {
                    sb.AvailableQuantity,
                    ExpiryDate = sb.ExpiryDate.Date
                })
                .ToListAsync(cancellationToken);

            var count = 0;

            foreach (var batch in batches)
            {
                if (batch.ExpiryDate <= today)
                {
                    count++;
                    continue;
                }

                if (batch.ExpiryDate <= expiringSoonDate)
                    count++;

                if (batch.AvailableQuantity <= 10)
                    count++;
            }

            return new NotificationsCountDto
            {
                Count = count
            };
        }
    }
}

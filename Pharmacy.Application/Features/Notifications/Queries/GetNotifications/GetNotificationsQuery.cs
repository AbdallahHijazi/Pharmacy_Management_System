using MediatR;
using Pharmacy.Application.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.Features.Notifications.Queries.GetNotifications
{
    public class GetNotificationsQuery : IRequest<List<NotificationDto>>
    {
    }
}

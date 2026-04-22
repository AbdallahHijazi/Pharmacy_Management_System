using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmacy.Application.DTOs.Notifications
{
    public class NotificationDto
    {
        public string Type { get; set; } = string.Empty; // LowStock / ExpiringSoon / Expired
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

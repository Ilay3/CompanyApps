using System;
using System.Collections.Generic;

namespace CompanyApp.Core.Entities
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }

        // Статус: New, InProgress, Resolved, Closed
        public string Status { get; set; }

        // Приоритет: Low, Medium, High, Critical
        public string Priority { get; set; }

        // Тип: Computer, Equipment, Software, Other
        public string RequestType { get; set; }

        // Связь с оборудованием
        public int? ComputerId { get; set; }
        public Computer? Computer { get; set; }

        public int? EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        // Пользователи
        public string CreatedByUserId { get; set; }
        public ApplicationUser CreatedByUser { get; set; }

        public string? AssignedToUserId { get; set; }
        public ApplicationUser? AssignedToUser { get; set; }

        // История изменений статуса
        public ICollection<ServiceRequestStatusHistory>? StatusHistories { get; set; }

        // Комментарии
        public ICollection<ServiceRequestComment>? Comments { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace CompanyApp.Application.DTOs
{
    public class ServiceRequestDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string RequestType { get; set; }

        public int? ComputerId { get; set; }
        public ComputerDto? Computer { get; set; }

        public int? EquipmentId { get; set; }
        public EquipmentDto? Equipment { get; set; }

        public string CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }

        public string? AssignedToUserId { get; set; }
        public string? AssignedToUserName { get; set; }

        public List<ServiceRequestStatusHistoryDto> StatusHistories { get; set; } = new List<ServiceRequestStatusHistoryDto>();
        public List<ServiceRequestCommentDto> Comments { get; set; } = new List<ServiceRequestCommentDto>();
    }

    public class CreateServiceRequestDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; } = "Medium";
        public string RequestType { get; set; }
        public int? ComputerId { get; set; }
        public int? EquipmentId { get; set; }
    }

    public class UpdateServiceRequestStatusDto
    {
        public int ServiceRequestId { get; set; }
        public string NewStatus { get; set; }
        public string? Reason { get; set; }
        public string? Resolution { get; set; }
    }

    public class ServiceRequestStatusHistoryDto
    {
        public int Id { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public string? Reason { get; set; }
        public string? Resolution { get; set; }
        public DateTime ChangedDate { get; set; }
        public string ChangedByUserName { get; set; }
    }

    public class ServiceRequestCommentDto
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }
    }

    public class AddCommentDto
    {
        public int ServiceRequestId { get; set; }
        public string Comment { get; set; }
    }
}
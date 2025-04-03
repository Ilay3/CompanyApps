using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.DTOs
{
    public class MaintenanceRecordDto
    {
        public int Id { get; set; }

        public int? ComputerId { get; set; }
        public ComputerDto? Computer { get; set; }

        public int? EquipmentId { get; set; }
        public EquipmentDto? Equipment { get; set; }

        [Required(ErrorMessage = "Дата обслуживания обязательна")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime MaintenanceDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? NextMaintenanceDate { get; set; }

        [Required(ErrorMessage = "Тип обслуживания обязателен")]
        public string MaintenanceType { get; set; } // Planned, Emergency, Warranty

        [Required(ErrorMessage = "Описание обязательно")]
        public string Description { get; set; }

        public string? TechnicianName { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Стоимость должна быть положительным числом")]
        public decimal? Cost { get; set; }

        [Required(ErrorMessage = "Статус обязателен")]
        public string Status { get; set; } // Pending, Completed, Cancelled

        public string? Notes { get; set; }
        public string? AttachmentPath { get; set; }
    }

    public class CreateMaintenanceRecordDto
    {
        public int? ComputerId { get; set; }
        public int? EquipmentId { get; set; }

        [Required(ErrorMessage = "Дата обслуживания обязательна")]
        [DataType(DataType.Date)]
        public DateTime MaintenanceDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime? NextMaintenanceDate { get; set; }

        [Required(ErrorMessage = "Тип обслуживания обязателен")]
        public string MaintenanceType { get; set; } = "Planned";

        [Required(ErrorMessage = "Описание обязательно")]
        public string Description { get; set; }

        public string? TechnicianName { get; set; }
        public decimal? Cost { get; set; }

        [Required(ErrorMessage = "Статус обязателен")]
        public string Status { get; set; } = "Pending";

        public string? Notes { get; set; }
    }

    public class MaintenanceSummaryDto
    {
        public int TotalRecords { get; set; }
        public int PendingRecords { get; set; }
        public int CompletedRecords { get; set; }
        public int CancelledRecords { get; set; }
        public decimal TotalCost { get; set; }
        public List<MaintenanceRecordDto> UpcomingMaintenance { get; set; } = new List<MaintenanceRecordDto>();
    }
}
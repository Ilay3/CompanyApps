using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Entities
{
    public class MaintenanceRecord
    {
        public int Id { get; set; }

        // Связь с оборудованием
        public int? ComputerId { get; set; }
        public Computer? Computer { get; set; }

        public int? EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        public DateTime MaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public string MaintenanceType { get; set; }
        public string Description { get; set; }
        public string? TechnicianName { get; set; }
        public decimal? Cost { get; set; }
        public string Status { get; set; }
        public string? Notes { get; set; }
        public string? AttachmentPath { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace CompanyApp.Application.DTOs
{
    public class EquipmentReportDto
    {
        public string Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Перемещения компьютеров
        public List<ComputerMovementDto> ComputerMovements { get; set; } = new List<ComputerMovementDto>();

        // Частые поломки
        public List<FrequentBreakdownDto> FrequentBreakdowns { get; set; } = new List<FrequentBreakdownDto>();

        // Причины поломок
        public List<BreakdownReasonDto> BreakdownReasons { get; set; } = new List<BreakdownReasonDto>();

        // Статистика по заявкам
        public ServiceRequestStatisticsDto ServiceRequestStatistics { get; set; }
    }

    public class ComputerMovementDto
    {
        public int ComputerId { get; set; }
        public string ComputerModel { get; set; }
        public string ComputerIdentifier { get; set; }
        public DateTime MovementDate { get; set; }
        public string FromEmployee { get; set; }
        public string ToEmployee { get; set; }
        public string FromDepartment { get; set; }
        public string ToDepartment { get; set; }
    }

    public class FrequentBreakdownDto
    {
        public string EquipmentType { get; set; }
        public string EquipmentModel { get; set; }
        public string EquipmentIdentifier { get; set; }
        public int BreakdownCount { get; set; }
        public decimal AverageRepairCost { get; set; }
        public List<string> CommonIssues { get; set; }
    }

    public class BreakdownReasonDto
    {
        public string Reason { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class ServiceRequestStatisticsDto
    {
        public int TotalRequests { get; set; }
        public int ResolvedRequests { get; set; }
        public int PendingRequests { get; set; }
        public double AverageResolutionTime { get; set; }
        public Dictionary<string, int> RequestsByType { get; set; }
        public Dictionary<string, int> RequestsByPriority { get; set; }
    }
}
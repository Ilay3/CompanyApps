using CompanyApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Models
{
    public class DashboardViewModel
    {
        public int TotalOffices { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalComputers { get; set; }
        public int TotalEquipment { get; set; }
        public int TotalSoftwareLicenses { get; set; }
        public int TotalLicenses { get; set; }
        public int UsedLicenses { get; set; }

        public List<OfficeDashboardViewModel> Offices { get; set; } = new List<OfficeDashboardViewModel>();
        public List<MaintenanceRecordDto> UpcomingMaintenance { get; set; } = new List<MaintenanceRecordDto>();
        public List<SoftwareLicenseDto> ExpiringSoftware { get; set; } = new List<SoftwareLicenseDto>();
        public List<ChartDataItem> EquipmentTypeData { get; set; } = new List<ChartDataItem>();
    }

    public class ChartDataItem
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }
}
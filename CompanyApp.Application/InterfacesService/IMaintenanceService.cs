using CompanyApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.InterfacesService
{
    public interface IMaintenanceService
    {
        Task<IEnumerable<MaintenanceRecordDto>> GetAllMaintenanceRecordsAsync();
        Task<MaintenanceRecordDto> GetMaintenanceRecordByIdAsync(int id);
        Task<MaintenanceRecordDto> CreateMaintenanceRecordAsync(CreateMaintenanceRecordDto maintenanceRecordDto);
        Task UpdateMaintenanceRecordAsync(MaintenanceRecordDto maintenanceRecordDto);
        Task DeleteMaintenanceRecordAsync(int id);
        Task<IEnumerable<MaintenanceRecordDto>> GetMaintenanceRecordsByComputerIdAsync(int computerId);
        Task<IEnumerable<MaintenanceRecordDto>> GetMaintenanceRecordsByEquipmentIdAsync(int equipmentId);
        Task<IEnumerable<MaintenanceRecordDto>> GetUpcomingMaintenanceAsync(int daysThreshold);
        Task<MaintenanceSummaryDto> GetMaintenanceSummaryAsync();
    }

}

using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.InterfacesRepository
{
    public interface IMaintenanceRepository
    {
        Task<IEnumerable<MaintenanceRecord>> GetAllMaintenanceRecordsAsync();
        Task<MaintenanceRecord> GetMaintenanceRecordByIdAsync(int id);
        Task<MaintenanceRecord> CreateMaintenanceRecordAsync(MaintenanceRecord maintenanceRecord);
        Task UpdateMaintenanceRecordAsync(MaintenanceRecord maintenanceRecord);
        Task DeleteMaintenanceRecordAsync(int id);
        Task<IEnumerable<MaintenanceRecord>> GetMaintenanceRecordsByComputerIdAsync(int computerId);
        Task<IEnumerable<MaintenanceRecord>> GetMaintenanceRecordsByEquipmentIdAsync(int equipmentId);
        Task<IEnumerable<MaintenanceRecord>> GetUpcomingMaintenanceAsync(DateTime dateThreshold);
        Task<MaintenanceRecord> UpdateMaintenanceStatusAsync(int id, string status);
    }

}

using CompanyApp.Core.Entities;
using CompanyApp.Infrastructure.Data;
using CompanyApp.Infrastructure.InterfacesRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.Repositories
{
    public class MaintenanceRepository : IMaintenanceRepository
    {
        private readonly OfficeDbContext _context;

        public MaintenanceRepository(OfficeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetAllMaintenanceRecordsAsync()
        {
            return await _context.MaintenanceRecords
                .Include(mr => mr.Computer)
                .Include(mr => mr.Equipment)
                .ToListAsync();
        }

        public async Task<MaintenanceRecord> GetMaintenanceRecordByIdAsync(int id)
        {
            return await _context.MaintenanceRecords
                .Include(mr => mr.Computer)
                .Include(mr => mr.Equipment)
                .FirstOrDefaultAsync(mr => mr.Id == id);
        }

        public async Task<MaintenanceRecord> CreateMaintenanceRecordAsync(MaintenanceRecord maintenanceRecord)
        {
            _context.MaintenanceRecords.Add(maintenanceRecord);
            await _context.SaveChangesAsync();
            return maintenanceRecord;
        }

        public async Task UpdateMaintenanceRecordAsync(MaintenanceRecord maintenanceRecord)
        {
            _context.Entry(maintenanceRecord).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMaintenanceRecordAsync(int id)
        {
            var maintenanceRecord = await _context.MaintenanceRecords.FindAsync(id);
            if (maintenanceRecord != null)
            {
                _context.MaintenanceRecords.Remove(maintenanceRecord);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetMaintenanceRecordsByComputerIdAsync(int computerId)
        {
            return await _context.MaintenanceRecords
                .Where(mr => mr.ComputerId == computerId)
                .Include(mr => mr.Computer)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetMaintenanceRecordsByEquipmentIdAsync(int equipmentId)
        {
            return await _context.MaintenanceRecords
                .Where(mr => mr.EquipmentId == equipmentId)
                .Include(mr => mr.Equipment)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceRecord>> GetUpcomingMaintenanceAsync(DateTime dateThreshold)
        {
            return await _context.MaintenanceRecords
                .Where(mr => mr.NextMaintenanceDate.HasValue &&
                            mr.NextMaintenanceDate <= dateThreshold &&
                            mr.Status != "Completed" &&
                            mr.Status != "Cancelled")
                .Include(mr => mr.Computer)
                .Include(mr => mr.Equipment)
                .ToListAsync();
        }

        public async Task<MaintenanceRecord> UpdateMaintenanceStatusAsync(int id, string status)
        {
            var maintenanceRecord = await _context.MaintenanceRecords.FindAsync(id);
            if (maintenanceRecord != null)
            {
                maintenanceRecord.Status = status;
                _context.Entry(maintenanceRecord).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            return maintenanceRecord;
        }
    }
}
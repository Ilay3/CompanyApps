using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Application.InterfacesService;
using CompanyApp.Core.Entities;
using CompanyApp.Core.Interfaces;
using CompanyApp.Infrastructure.InterfacesRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IMapper _mapper;

        public MaintenanceService(IMaintenanceRepository maintenanceRepository, IMapper mapper)
        {
            _maintenanceRepository = maintenanceRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MaintenanceRecordDto>> GetAllMaintenanceRecordsAsync()
        {
            var maintenanceRecords = await _maintenanceRepository.GetAllMaintenanceRecordsAsync();
            return _mapper.Map<IEnumerable<MaintenanceRecordDto>>(maintenanceRecords);
        }

        public async Task<MaintenanceRecordDto> GetMaintenanceRecordByIdAsync(int id)
        {
            var maintenanceRecord = await _maintenanceRepository.GetMaintenanceRecordByIdAsync(id);
            return _mapper.Map<MaintenanceRecordDto>(maintenanceRecord);
        }

        public async Task<MaintenanceRecordDto> CreateMaintenanceRecordAsync(CreateMaintenanceRecordDto createMaintenanceRecordDto)
        {
            var maintenanceRecord = _mapper.Map<MaintenanceRecord>(createMaintenanceRecordDto);
            maintenanceRecord = await _maintenanceRepository.CreateMaintenanceRecordAsync(maintenanceRecord);
            return _mapper.Map<MaintenanceRecordDto>(maintenanceRecord);
        }

        public async Task UpdateMaintenanceRecordAsync(MaintenanceRecordDto maintenanceRecordDto)
        {
            var maintenanceRecord = _mapper.Map<MaintenanceRecord>(maintenanceRecordDto);
            await _maintenanceRepository.UpdateMaintenanceRecordAsync(maintenanceRecord);
        }

        public async Task DeleteMaintenanceRecordAsync(int id)
        {
            await _maintenanceRepository.DeleteMaintenanceRecordAsync(id);
        }

        public async Task<IEnumerable<MaintenanceRecordDto>> GetMaintenanceRecordsByComputerIdAsync(int computerId)
        {
            var maintenanceRecords = await _maintenanceRepository.GetMaintenanceRecordsByComputerIdAsync(computerId);
            return _mapper.Map<IEnumerable<MaintenanceRecordDto>>(maintenanceRecords);
        }

        public async Task<IEnumerable<MaintenanceRecordDto>> GetMaintenanceRecordsByEquipmentIdAsync(int equipmentId)
        {
            var maintenanceRecords = await _maintenanceRepository.GetMaintenanceRecordsByEquipmentIdAsync(equipmentId);
            return _mapper.Map<IEnumerable<MaintenanceRecordDto>>(maintenanceRecords);
        }

        public async Task<IEnumerable<MaintenanceRecordDto>> GetUpcomingMaintenanceAsync(int daysThreshold)
        {
            var dateThreshold = DateTime.Now.AddDays(daysThreshold);
            var upcomingMaintenance = await _maintenanceRepository.GetUpcomingMaintenanceAsync(dateThreshold);
            return _mapper.Map<IEnumerable<MaintenanceRecordDto>>(upcomingMaintenance);
        }

        public async Task<MaintenanceSummaryDto> GetMaintenanceSummaryAsync()
        {
            var allMaintenanceRecords = await _maintenanceRepository.GetAllMaintenanceRecordsAsync();

            var summary = new MaintenanceSummaryDto
            {
                TotalRecords = allMaintenanceRecords.Count(),
                PendingRecords = allMaintenanceRecords.Count(r => r.Status == "Pending"),
                CompletedRecords = allMaintenanceRecords.Count(r => r.Status == "Completed"),
                CancelledRecords = allMaintenanceRecords.Count(r => r.Status == "Cancelled"),
                TotalCost = allMaintenanceRecords.Where(r => r.Cost.HasValue).Sum(r => r.Cost.Value)
            };

            // Получаем предстоящее обслуживание на ближайшие 30 дней
            var upcomingMaintenance = await GetUpcomingMaintenanceAsync(30);
            summary.UpcomingMaintenance = upcomingMaintenance.ToList();

            return summary;
        }
    }
}
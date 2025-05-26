using CompanyApp.Application.DTOs;
using CompanyApp.Application.InterfacesService;
using CompanyApp.Core.Interfaces;
using CompanyApp.Infrastructure.Data;
using CompanyApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly OfficeDbContext _context;
        private readonly IOfficeService _officeService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly ISoftwareLicenseService _softwareLicenseService;

        public DashboardController(
            OfficeDbContext context,
            IOfficeService officeService,
            IMaintenanceService maintenanceService,
            ISoftwareLicenseService softwareLicenseService)
        {
            _context = context;
            _officeService = officeService;
            _maintenanceService = maintenanceService;
            _softwareLicenseService = softwareLicenseService;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            var offices = await _context.Offices.ToListAsync();
            var dashboardViewModel = new DashboardViewModel();

            // Общая статистика
            dashboardViewModel.TotalOffices = offices.Count;
            dashboardViewModel.TotalDepartments = await _context.Departments.CountAsync();
            dashboardViewModel.TotalEmployees = await _context.Employees.CountAsync();
            dashboardViewModel.TotalComputers = await _context.Computers.CountAsync();
            dashboardViewModel.TotalEquipment = await _context.Equipments.CountAsync();
            dashboardViewModel.TotalSoftwareLicenses = await _context.SoftwareLicenses.CountAsync();

            // Данные по офисам
            foreach (var office in offices)
            {
                // Получаем количество департаментов
                var departmentCount = await _context.Departments.CountAsync(d => d.OfficeId == office.Id);

                // Получаем количество сотрудников
                var employeeCount = await _context.Employees.CountAsync(e => e.Department.OfficeId == office.Id);

                // Получаем количество компьютеров
                var computerCount = await _context.Computers.CountAsync(c => c.Employee.Department.OfficeId == office.Id);

                // Получаем количество оборудования
                var equipmentCount = await _context.Equipments.CountAsync(e => e.Department.OfficeId == office.Id);

                // Создаем ViewModel для офиса
                var officeDashboard = new OfficeDashboardViewModel
                {
                    OfficeId = office.Id,
                    OfficeName = office.Name,
                    City = office.City,
                    DepartmentCount = departmentCount,
                    EmployeeCount = employeeCount,
                    ComputerCount = computerCount,
                    EquipmentCount = equipmentCount
                };

                dashboardViewModel.Offices.Add(officeDashboard);
            }

            // Получаем данные о предстоящем обслуживании
            var upcomingMaintenance = await _maintenanceService.GetUpcomingMaintenanceAsync(30);
            dashboardViewModel.UpcomingMaintenance = upcomingMaintenance.Take(5).ToList();

            // Получаем данные о программном обеспечении с истекающими лицензиями
            var expiringSoftware = await _softwareLicenseService.GetExpiringSoftwareLicensesAsync(30);
            dashboardViewModel.ExpiringSoftware = expiringSoftware.Take(5).ToList();

            // Подсчитываем свободные и используемые лицензии
            var softwareLicenses = await _context.SoftwareLicenses.ToListAsync();
            dashboardViewModel.TotalLicenses = softwareLicenses.Sum(s => s.Seats);
            dashboardViewModel.UsedLicenses = softwareLicenses.Sum(s => s.SeatsUsed);

            // Собираем данные о типах оборудования для построения диаграммы
            var equipmentTypes = await _context.Equipments
                .GroupBy(e => e.Type)
                .Select(g => new ChartDataItem
                {
                    Label = g.Key,
                    Value = g.Count()
                })
                .ToListAsync();

            dashboardViewModel.EquipmentTypeData = equipmentTypes;

            return View(dashboardViewModel);
        }
    }
}
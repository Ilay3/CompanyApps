using CompanyApp.Application.DTOs;
using CompanyApp.Core.Interfaces;
using CompanyApp.Infrastructure.Data;
using CompanyApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompanyApp.Controllers
{
    public class OfficeController : Controller
    {
        private readonly IOfficeService _officeService;
        private readonly IBuildingService _buildingService;
        private readonly IDepartmentService _departmentService;
        private readonly OfficeDbContext _context;


        public OfficeController(IOfficeService officeService, IBuildingService buildingService, IDepartmentService departmentService, OfficeDbContext context)
        {
            _officeService = officeService;
            _buildingService = buildingService;
            _departmentService = departmentService;
            _context = context;
        }

        // GET: /Office
        public async Task<IActionResult> Index()
        {
            // Получаем все офисы
            var offices = await _context.Offices.ToListAsync();
            var officeDashboards = new List<OfficeDashboardViewModel>();

            foreach (var office in offices)
            {
                // Получаем количество департаментов
                var departmentCount = await _context.Departments
                    .CountAsync(d => d.OfficeId == office.Id);

                // Получаем количество сотрудников
                var employeeCount = await _context.Employees
                    .CountAsync(e => e.Department.OfficeId == office.Id);

                // Получаем количество компьютеров
                var computerCount = await _context.Computers
                    .CountAsync(c => c.Employee.Department.OfficeId == office.Id);

                // Получаем количество оборудования
                var equipmentCount = await _context.Equipments
                    .CountAsync(e => e.Department.OfficeId == office.Id);

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

                officeDashboards.Add(officeDashboard);
            }

            return View(officeDashboards);
        }


        // GET: /Office/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var office = await _officeService.GetOfficeByIdAsync(id);
            if (office == null)
            {
                return NotFound();
            }

            if (!office.Buildings.Any())
            {
                var departments = await _departmentService.GetDepartmentsByOfficeIdAsync(id);
                office.Departments = departments.ToList();
            }

            return View(office);
        }

        // GET: /Office/Buildings/5
        public async Task<IActionResult> Buildings(int id)
        {
            var buildings = await _buildingService.GetBuildingsByOfficeIdAsync(id);
            if (!buildings.Any())
            {
                // No buildings, so fetch departments directly
                var departments = await _departmentService.GetDepartmentsByOfficeIdAsync(id);
                return View("Departments", departments);
            }

            return View(buildings);
        }

        public async Task<IActionResult> Departments(int id)
        {
            var departments = await _departmentService.GetDepartmentsByOfficeIdAsync(id);
            if (departments == null)
            {
                return NotFound();
            }

            return View(departments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new OfficeDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(OfficeDto officeDto)
        {
            if (!ModelState.IsValid)
            {
                return View(officeDto);
            }

            var officeId = await _officeService.CreateOfficeAsync(officeDto);

            return RedirectToAction("Details", new { id = officeId });
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var officeDto = await _officeService.GetOfficeByIdAsync(id);
            if (officeDto == null)
            {
                return NotFound();
            }
            return PartialView("_EditOfficePartial", officeDto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(OfficeDto officeDto)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditOfficePartial", officeDto);
            }

            await _officeService.UpdateOfficeAsync(officeDto);
            return RedirectToAction("Index");
        }

    }

}

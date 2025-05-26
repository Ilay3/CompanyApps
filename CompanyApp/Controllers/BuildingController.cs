using CompanyApp.Application.DTOs;
using CompanyApp.Application.Services;
using CompanyApp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CompanyApp.Controllers
{
    public class BuildingController : Controller
    {
        private readonly IBuildingService _buildingService;
        private readonly IDepartmentService _departmentService;
        private readonly IOfficeService _officeService;

        public BuildingController(IBuildingService buildingService, IDepartmentService departmentService, IOfficeService officeService)
        {
            _buildingService = buildingService;
            _departmentService = departmentService;
            _officeService = officeService;
        }

        // GET: /Building/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var building = await _buildingService.GetBuildingByIdAsync(id);
            if (building == null)
            {
                return NotFound();
            }

            var departments = await _departmentService.GetDepartmentsByBuildingIdAsync(id);

            // Сортируем departments по Name: сначала числа (1-9), затем буквы (А-Я)
            departments = departments
                .OrderBy(d => char.IsDigit(d.Name.First()) ? 0 : 1) // Приоритет числам
                .ThenBy(d => d.Name, StringComparer.CurrentCultureIgnoreCase) // Сортировка по имени
                .ToList();

            building.Departments = departments;

            return View(building);
        }

        // GET: /Building/Departments/5
        public async Task<IActionResult> Departments(int id)
        {
            var departments = await _departmentService.GetDepartmentsByBuildingIdAsync(id);
            if (departments == null)
            {
                return NotFound();
            }

            return View(departments);
        }

        
    }
}




using CompanyApp.Application.DTOs;
using CompanyApp.Application.Services;
using CompanyApp.Core.Interfaces;
using CompanyApp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CompanyApp.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly OfficeDbContext _context;


        public EmployeeController(IEmployeeService employeeService, IDepartmentService departmentService, OfficeDbContext context)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _context = context;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        // GET: /Employee/Create?departmentId=2
        [HttpGet]
        public async Task<IActionResult> Create(int departmentId)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(departmentId);
            if (department == null)
            {
                return NotFound();
            }

            // Передаем идентификатор отдела в модель
            var model = new EmployeeDto
            {
                DepartmentId = departmentId
            };

            return View(model);
        }

        // POST: /Employee/Create
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeDto employeeDto)
        {
            if (!ModelState.IsValid)
            {
                return View(employeeDto);
            }

            // Добавляем сотрудника и компьютеры (включая генерацию UniqueIdentifier)
            await _employeeService.AddEmployeeAsync(employeeDto);

            // Перенаправляем на страницу департамента
            return RedirectToAction("Details", "Department", new { id = employeeDto.DepartmentId });
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id, int departmentId)
        {
            // Получаем сотрудника по его ID
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound(); // Если сотрудник не найден
            }

            // Заполняем модель DTO (или можно напрямую использовать модель)
            var employeeDto = new EmployeeDto
            {
                Id = employee.Id,
                FullName = employee.FullName,
                DepartmentId = employee.DepartmentId
            };

            // Передаем DTO в представление
            return View(employeeDto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EmployeeDto employeeDto)
        {
            if (!ModelState.IsValid)
            {
                return View(employeeDto); // Если данные невалидны, возвращаем форму с ошибками
            }

            // Получаем сотрудника по его ID
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == employeeDto.Id);
            if (employee == null)
            {
                return NotFound(); // Если сотрудник не найден
            }

            // Обновляем поля
            employee.FullName = employeeDto.FullName;
            employee.DepartmentId = employeeDto.DepartmentId; // Присваиваем ID отдела

            // Пробуем сохранить изменения
            try
            {
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync(); // Сохраняем изменения
            }
            catch (Exception ex)
            {
                // Обработка ошибки
                ModelState.AddModelError("", "Ошибка при сохранении данных.");
                return View(employeeDto); // Возвращаем форму с ошибкой
            }

            // После успешного сохранения, перенаправляем на страницу отдела
            return RedirectToAction("Details", "Department", new { id = employeeDto.DepartmentId });
        }

        // Кнопка отмены (через GET-запрос)
        [HttpGet]
        public IActionResult Cancel(int departmentId)
        {
            // Перенаправляем на страницу деталей отдела
            return RedirectToAction("Details", "Department", new { id = departmentId });
        }






        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return NoContent();
        }
    }

}

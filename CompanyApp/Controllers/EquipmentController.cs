// ПОЛНОСТЬЮ ОБНОВЛЕННЫЙ EquipmentController с безопасным удалением

using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Application.Services;
using CompanyApp.Core.Entities;
using CompanyApp.Core.Interfaces;
using CompanyApp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CompanyApp.Controllers
{
    public class EquipmentController : Controller
    {
        private readonly IEquipmentService _equipmentService;
        private readonly IDepartmentService _departmentService;
        private readonly IMapper _mapper;
        private readonly IBuildingService _buildingService;
        private readonly IOfficeService _officeService;
        private readonly OfficeDbContext _context;

        public EquipmentController(
            IEquipmentService equipmentService,
            IDepartmentService departmentService,
            IMapper mapper,
            IBuildingService buildingService,
            IOfficeService officeService,
            OfficeDbContext context)
        {
            _equipmentService = equipmentService;
            _departmentService = departmentService;
            _mapper = mapper;
            _buildingService = buildingService;
            _officeService = officeService;
            _context = context;
        }

        // GET: /Equipment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }

        // GET: /Equipment/Create
        public async Task<IActionResult> Create(int departmentId)
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            var dto = new CRUDEquipmentDto { DepartmentId = departmentId };

            return View(dto);
        }

        // POST: /Equipment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CRUDEquipmentDto dto)
        {
            if (ModelState.IsValid)
            {
                var createdEquipment = await _equipmentService.CreateEquipmentAsync(dto);
                return RedirectToAction("Details", "Department", new { id = dto.DepartmentId });
            }

            var departments = await _departmentService.GetAllDepartmentsAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name");

            return View(dto);
        }

        // GET: /Equipment/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
            if (equipment == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<CRUDEquipmentDto>(equipment);
            return View(dto);
        }

        // POST: /Equipment/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CRUDEquipmentDto dto)
        {
            if (ModelState.IsValid)
            {
                await _equipmentService.UpdateEquipmentAsync(dto);
                return RedirectToAction("Details", "Department", new { id = dto.DepartmentId });
            }

            return View(dto);
        }

        // POST: /Equipment/DeleteFromDepartment/5 - удаление из списка отдела
        [HttpPost]
        [Route("Equipment/DeleteFromDepartment/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFromDepartment(int id, int departmentId)
        {
            try
            {
                // Проверяем зависимости перед удалением
                var canDelete = await CheckCanDeleteEquipmentAsync(id);
                if (!canDelete.CanDelete)
                {
                    TempData["Error"] = canDelete.Message;
                    return RedirectToAction("Details", "Department", new { id = departmentId });
                }

                await _equipmentService.DeleteEquipmentAsync(id);
                TempData["Success"] = "Оборудование успешно удалено.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при удалении оборудования: {ex.Message}";
            }

            return RedirectToAction("Details", "Department", new { id = departmentId });
        }

        // POST: /Equipment/Delete/5 - удаление со страницы деталей
        [HttpPost]
        [Route("Equipment/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
            if (equipment == null)
            {
                return NotFound();
            }

            var departmentId = equipment.DepartmentId;

            try
            {
                // Проверяем зависимости перед удалением
                var canDelete = await CheckCanDeleteEquipmentAsync(id);
                if (!canDelete.CanDelete)
                {
                    TempData["Error"] = canDelete.Message;
                    return RedirectToAction("Details", "Department", new { id = departmentId });
                }

                await _equipmentService.DeleteEquipmentAsync(id);
                TempData["Success"] = "Оборудование успешно удалено.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при удалении оборудования: {ex.Message}";
            }

            return RedirectToAction("Details", "Department", new { id = departmentId });
        }

        // GET: /Equipment/DeleteConfirmation/5 - страница подтверждения удаления
        [HttpGet]
        [Route("Equipment/DeleteConfirmation/{id:int}")]
        public async Task<IActionResult> DeleteConfirmation(int id)
        {
            var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
            if (equipment == null)
            {
                return NotFound();
            }

            // Проверяем, можно ли удалить
            var canDelete = await CheckCanDeleteEquipmentAsync(id);
            ViewBag.CanDelete = canDelete.CanDelete;
            ViewBag.DeleteMessage = canDelete.Message;

            return View("Delete", equipment);
        }

        // Вспомогательный метод для проверки возможности удаления
        private async Task<(bool CanDelete, string Message)> CheckCanDeleteEquipmentAsync(int equipmentId)
        {
            // Проверяем записи обслуживания
            var hasMaintenanceRecords = await _context.MaintenanceRecords
                .AnyAsync(mr => mr.EquipmentId == equipmentId);

            if (hasMaintenanceRecords)
            {
                return (false, "Невозможно удалить оборудование, так как для него есть записи технического обслуживания.");
            }

            // Проверяем заявки на обслуживание
            var hasServiceRequests = await _context.ServiceRequests
                .AnyAsync(sr => sr.EquipmentId == equipmentId);

            if (hasServiceRequests)
            {
                return (false, "Невозможно удалить оборудование, так как для него есть заявки на обслуживание.");
            }

            return (true, string.Empty);
        }
    }
}
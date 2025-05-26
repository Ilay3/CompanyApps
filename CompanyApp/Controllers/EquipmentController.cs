using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Application.Services;
using CompanyApp.Core.Entities;
using CompanyApp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CompanyApp.Controllers
{
    public class EquipmentController : Controller
    {
        private readonly IEquipmentService _equipmentService;
        private readonly IDepartmentService _departmentService;
        private readonly IMapper _mapper;
        private readonly IBuildingService _buildingService;
        private readonly IOfficeService _officeService;

        public EquipmentController(
            IEquipmentService equipmentService,
            IDepartmentService departmentService,
            IMapper mapper,
            IBuildingService buildingService,
            IOfficeService officeService)
        {
            _equipmentService = equipmentService;
            _departmentService = departmentService;
            _mapper = mapper;
            _buildingService = buildingService;
            _officeService = officeService;
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

            // Маппинг с помощью AutoMapper, если доступен
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


        // POST: /Equipment/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int departmentId)
        {
            await _equipmentService.DeleteEquipmentAsync(id);
            return RedirectToAction("Details", "Department", new { id = departmentId });
        }
    }

}
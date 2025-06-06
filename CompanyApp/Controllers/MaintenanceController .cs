

using CompanyApp.Application.DTOs;
using CompanyApp.Application.InterfacesService;
using CompanyApp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CompanyApp.Controllers
{
    [Route("[controller]")]
    public class MaintenanceController : Controller
    {
        private readonly IMaintenanceService _maintenanceService;
        private readonly IComputerService _computerService;
        private readonly IEquipmentService _equipmentService;

        public MaintenanceController(
            IMaintenanceService maintenanceService,
            IComputerService computerService,
            IEquipmentService equipmentService)
        {
            _maintenanceService = maintenanceService;
            _computerService = computerService;
            _equipmentService = equipmentService;
        }

        // GET: Maintenance
        [HttpGet]
        [HttpGet("Index")]
        [Route("~/Maintenance")]
        public async Task<IActionResult> Index()
        {
            var maintenanceRecords = await _maintenanceService.GetAllMaintenanceRecordsAsync();
            return View(maintenanceRecords);
        }

        // GET: Maintenance/Details/5
        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var maintenanceRecord = await _maintenanceService.GetMaintenanceRecordByIdAsync(id);
            if (maintenanceRecord == null)
            {
                return NotFound();
            }

            return View(maintenanceRecord);
        }

        // GET: Maintenance/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int? computerId = null, int? equipmentId = null)
        {
            var createDto = new CreateMaintenanceRecordDto
            {
                ComputerId = computerId,
                EquipmentId = equipmentId,
                MaintenanceDate = DateTime.Now,
                MaintenanceType = "Planned",
                Status = "Pending"
            };

            if (computerId.HasValue)
            {
                var computer = await _computerService.GetComputerByIdAsync(computerId.Value);
                ViewBag.ItemName = $"Компьютер: {computer?.Model}";
                ViewBag.ItemType = "Computer";
            }
            else if (equipmentId.HasValue)
            {
                var equipment = await _equipmentService.GetEquipmentByIdAsync(equipmentId.Value);
                ViewBag.ItemName = $"Оборудование: {equipment?.Type} {equipment?.Model}";
                ViewBag.ItemType = "Equipment";
            }
            else
            {
                var computers = await _computerService.GetComputersByOfficeIdAsync(1);
                ViewBag.Computers = new SelectList(computers, "Id", "Model");

                try
                {
                    var allEquipment = await _equipmentService.GetAllEquipmentsAsync();
                    ViewBag.Equipments = new SelectList(allEquipment.Select(e => new {
                        Id = e.Id,
                        Model = $"{e.Type} - {e.Model}"
                    }), "Id", "Model");
                }
                catch
                {
                    ViewBag.Equipments = new SelectList(new List<object>(), "Id", "Model");
                }

                ViewBag.ItemType = "None";
            }

            return View(createDto);
        }

        // POST: Maintenance/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(CreateMaintenanceRecordDto createDto)
        {
            if (ModelState.IsValid)
            {
                var recordDto = await _maintenanceService.CreateMaintenanceRecordAsync(createDto);

                if (createDto.ComputerId.HasValue)
                {
                    return RedirectToAction("Details", "Computer", new { id = createDto.ComputerId.Value });
                }
                else if (createDto.EquipmentId.HasValue)
                {
                    return RedirectToAction("Details", "Equipment", new { id = createDto.EquipmentId.Value });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            // При ошибке возвращаем форму
            return View("Create", createDto);
        }

        // GET: Maintenance/Edit/5
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var maintenanceRecord = await _maintenanceService.GetMaintenanceRecordByIdAsync(id);
            if (maintenanceRecord == null)
            {
                return NotFound();
            }

            if (maintenanceRecord.ComputerId.HasValue)
            {
                var computer = await _computerService.GetComputerByIdAsync(maintenanceRecord.ComputerId.Value);
                ViewBag.ItemName = $"Компьютер: {computer?.Model}";
                ViewBag.ItemType = "Computer";
            }
            else if (maintenanceRecord.EquipmentId.HasValue)
            {
                var equipment = await _equipmentService.GetEquipmentByIdAsync(maintenanceRecord.EquipmentId.Value);
                ViewBag.ItemName = $"Оборудование: {equipment?.Type} {equipment?.Model}";
                ViewBag.ItemType = "Equipment";
            }

            return View(maintenanceRecord);
        }

        // POST: Maintenance/Edit/5
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, MaintenanceRecordDto maintenanceRecordDto)
        {
            if (id != maintenanceRecordDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _maintenanceService.UpdateMaintenanceRecordAsync(maintenanceRecordDto);

                if (maintenanceRecordDto.ComputerId.HasValue)
                {
                    return RedirectToAction("Details", "Computer", new { id = maintenanceRecordDto.ComputerId.Value });
                }
                else if (maintenanceRecordDto.EquipmentId.HasValue)
                {
                    return RedirectToAction("Details", "Equipment", new { id = maintenanceRecordDto.EquipmentId.Value });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View("Edit", maintenanceRecordDto);
        }

        // POST: Maintenance/MarkAsCompleted/5
        [HttpPost("MarkAsCompleted/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsCompleted(int id)
        {
            try
            {
                var maintenanceRecord = await _maintenanceService.GetMaintenanceRecordByIdAsync(id);
                if (maintenanceRecord == null)
                {
                    TempData["Error"] = "Запись о техническом обслуживании не найдена";
                    return RedirectToAction(nameof(Index));
                }

                maintenanceRecord.Status = "Completed";
                await _maintenanceService.UpdateMaintenanceRecordAsync(maintenanceRecord);

                TempData["Success"] = "Техническое обслуживание помечено как завершенное";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при обновлении статуса: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: Maintenance/MarkAsCancelled/5  
        [HttpPost("MarkAsCancelled/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsCancelled(int id)
        {
            try
            {
                var maintenanceRecord = await _maintenanceService.GetMaintenanceRecordByIdAsync(id);
                if (maintenanceRecord == null)
                {
                    TempData["Error"] = "Запись о техническом обслуживании не найдена";
                    return RedirectToAction(nameof(Index));
                }

                maintenanceRecord.Status = "Cancelled";
                await _maintenanceService.UpdateMaintenanceRecordAsync(maintenanceRecord);

                TempData["Success"] = "Техническое обслуживание отменено";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при обновлении статуса: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Maintenance/Delete/5
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var maintenanceRecord = await _maintenanceService.GetMaintenanceRecordByIdAsync(id);
            if (maintenanceRecord == null)
            {
                return NotFound();
            }
            return View(maintenanceRecord);
        }

        // POST: Maintenance/Delete/5
        [HttpPost("Delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maintenanceRecord = await _maintenanceService.GetMaintenanceRecordByIdAsync(id);
            await _maintenanceService.DeleteMaintenanceRecordAsync(id);

            if (maintenanceRecord.ComputerId.HasValue)
            {
                return RedirectToAction("Details", "Computer", new { id = maintenanceRecord.ComputerId.Value });
            }
            else if (maintenanceRecord.EquipmentId.HasValue)
            {
                return RedirectToAction("Details", "Equipment", new { id = maintenanceRecord.EquipmentId.Value });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Maintenance/Dashboard
        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var summary = await _maintenanceService.GetMaintenanceSummaryAsync();
            return View(summary);
        }

        // GET: Maintenance/Upcoming
        [HttpGet("Upcoming")]
        public async Task<IActionResult> Upcoming(int days = 30)
        {
            var upcomingMaintenance = await _maintenanceService.GetUpcomingMaintenanceAsync(days);
            ViewBag.Days = days;
            return View(upcomingMaintenance);
        }
    }
}
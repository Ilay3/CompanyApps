using CompanyApp.Application.DTOs;
using CompanyApp.Application.InterfacesService;
using CompanyApp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CompanyApp.API.Controllers
{
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

        // GET: /Maintenance
        public async Task<IActionResult> Index()
        {
            var maintenanceRecords = await _maintenanceService.GetAllMaintenanceRecordsAsync();
            return View(maintenanceRecords);
        }

        // GET: /Maintenance/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var maintenanceRecord = await _maintenanceService.GetMaintenanceRecordByIdAsync(id);
            if (maintenanceRecord == null)
            {
                return NotFound();
            }

            return View(maintenanceRecord);
        }

        // GET: /Maintenance/Create
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

            // Если задан computerId, получаем данные компьютера
            if (computerId.HasValue)
            {
                var computer = await _computerService.GetComputerByIdAsync(computerId.Value);
                ViewBag.ItemName = $"Компьютер: {computer?.Model}";
                ViewBag.ItemType = "Computer";
            }
            // Если задан equipmentId, получаем данные оборудования
            else if (equipmentId.HasValue)
            {
                var equipment = await _equipmentService.GetEquipmentByIdAsync(equipmentId.Value);
                ViewBag.ItemName = $"Оборудование: {equipment?.Type} {equipment?.Model}";
                ViewBag.ItemType = "Equipment";
            }
            else
            {
                // Загружаем списки компьютеров и оборудования для выпадающих списков
                var computers = await _computerService.GetComputersByOfficeIdAsync(1); // Заглушка
                ViewBag.Computers = new SelectList(computers, "Id", "Model");

                var equipments = new List<CRUDEquipmentDto>(); // Заглушка, в реальном приложении нужно получить список оборудования
                ViewBag.Equipments = new SelectList(equipments, "Id", "Model");

                ViewBag.ItemType = "None";
            }

            return View(createDto);
        }

        // POST: /Maintenance/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMaintenanceRecordDto createDto)
        {
            if (ModelState.IsValid)
            {
                var recordDto = await _maintenanceService.CreateMaintenanceRecordAsync(createDto);

                // Перенаправляем в зависимости от типа обслуживаемого оборудования
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

            // В случае ошибки валидации, возвращаем представление с теми же данными
            if (createDto.ComputerId.HasValue)
            {
                var computer = await _computerService.GetComputerByIdAsync(createDto.ComputerId.Value);
                ViewBag.ItemName = $"Компьютер: {computer?.Model}";
                ViewBag.ItemType = "Computer";
            }
            else if (createDto.EquipmentId.HasValue)
            {
                var equipment = await _equipmentService.GetEquipmentByIdAsync(createDto.EquipmentId.Value);
                ViewBag.ItemName = $"Оборудование: {equipment?.Type} {equipment?.Model}";
                ViewBag.ItemType = "Equipment";
            }
            else
            {
                var computers = await _computerService.GetComputersByOfficeIdAsync(1); // Заглушка
                ViewBag.Computers = new SelectList(computers, "Id", "Model");

                var equipments = new List<CRUDEquipmentDto>(); // Заглушка
                ViewBag.Equipments = new SelectList(equipments, "Id", "Model");

                ViewBag.ItemType = "None";
            }

            return View(createDto);
        }

        // GET: /Maintenance/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var maintenanceRecord = await _maintenanceService.GetMaintenanceRecordByIdAsync(id);
            if (maintenanceRecord == null)
            {
                return NotFound();
            }

            // Загружаем название обслуживаемого оборудования для отображения
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

        // POST: /Maintenance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaintenanceRecordDto maintenanceRecordDto)
        {
            if (id != maintenanceRecordDto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _maintenanceService.UpdateMaintenanceRecordAsync(maintenanceRecordDto);

                // Перенаправляем в зависимости от типа обслуживаемого оборудования
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

            // В случае ошибки валидации
            if (maintenanceRecordDto.ComputerId.HasValue)
            {
                var computer = await _computerService.GetComputerByIdAsync(maintenanceRecordDto.ComputerId.Value);
                ViewBag.ItemName = $"Компьютер: {computer?.Model}";
                ViewBag.ItemType = "Computer";
            }
            else if (maintenanceRecordDto.EquipmentId.HasValue)
            {
                var equipment = await _equipmentService.GetEquipmentByIdAsync(maintenanceRecordDto.EquipmentId.Value);
                ViewBag.ItemName = $"Оборудование: {equipment?.Type} {equipment?.Model}";
                ViewBag.ItemType = "Equipment";
            }

            return View(maintenanceRecordDto);
        }

        // GET: /Maintenance/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var maintenanceRecord = await _maintenanceService.GetMaintenanceRecordByIdAsync(id);
            if (maintenanceRecord == null)
            {
                return NotFound();
            }
            return View(maintenanceRecord);
        }

        // POST: /Maintenance/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maintenanceRecord = await _maintenanceService.GetMaintenanceRecordByIdAsync(id);
            await _maintenanceService.DeleteMaintenanceRecordAsync(id);

            // Перенаправляем в зависимости от типа обслуживаемого оборудования
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

        // GET: /Maintenance/ForComputer/5
        public async Task<IActionResult> ForComputer(int id)
        {
            var maintenanceRecords = await _maintenanceService.GetMaintenanceRecordsByComputerIdAsync(id);
            var computer = await _computerService.GetComputerByIdAsync(id);

            ViewBag.ComputerId = id;
            ViewBag.ComputerModel = computer?.Model;

            return View(maintenanceRecords);
        }

        // GET: /Maintenance/ForEquipment/5
        public async Task<IActionResult> ForEquipment(int id)
        {
            var maintenanceRecords = await _maintenanceService.GetMaintenanceRecordsByEquipmentIdAsync(id);
            var equipment = await _equipmentService.GetEquipmentByIdAsync(id);

            ViewBag.EquipmentId = id;
            ViewBag.EquipmentModel = equipment?.Model;

            return View(maintenanceRecords);
        }

        // GET: /Maintenance/Upcoming
        public async Task<IActionResult> Upcoming(int days = 30)
        {
            var upcomingMaintenance = await _maintenanceService.GetUpcomingMaintenanceAsync(days);
            ViewBag.Days = days;
            return View(upcomingMaintenance);
        }

        // GET: /Maintenance/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var summary = await _maintenanceService.GetMaintenanceSummaryAsync();
            return View(summary);
        }
    }
}
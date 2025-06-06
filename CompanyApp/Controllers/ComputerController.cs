using AutoMapper;
using ClosedXML.Excel;
using CompanyApp.Application.DTOs;
using CompanyApp.Core.Entities;
using CompanyApp.Core.Interfaces;
using CompanyApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;



namespace CompanyApp.Controllers
{
    [Authorize]
    public class ComputerController : Controller
    {
        private readonly OfficeDbContext _context;
        private readonly IMapper _mapper;
        private readonly IComputerService _computerService;
        private readonly IEmployeeService _employeeService;
       

        public ComputerController(OfficeDbContext context, IMapper mapper, IComputerService computerService, IEmployeeService employeeService)
        {
            _context = context;
            _mapper = mapper;
            _computerService = computerService;
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetComputersByOfficeId(int officeId, string searchTerm = "", int? departmentId = null, string? processor = null)
        {
            // Получаем список компьютеров с учетом фильтрации и поиска
            var query = _context.Computers
                .Include(c => c.Employee)
                .ThenInclude(e => e.Department)
                .Include(c => c.DisplayMonitors)
                .Where(c => c.Employee.Department.OfficeId == officeId);

            // Поиск по номеру, модели, процессору и сотруднику
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c => c.UniqueIdentifier.ToLower().Contains(searchTerm) ||
                                         c.Model.ToLower().Contains(searchTerm) ||
                                         c.Processor.ToLower().Contains(searchTerm) ||
                                         (c.Employee != null && c.Employee.FullName.ToLower().Contains(searchTerm)));
            }

            // Фильтрация по отделу
            if (departmentId.HasValue)
            {
                query = query.Where(c => c.Employee.DepartmentId == departmentId.Value);
            }

            // Фильтрация по процессору
            if (!string.IsNullOrEmpty(processor))
            {
                processor = processor.ToLower();
                query = query.Where(c => c.Processor.ToLower().Contains(processor));
            }

            var computers = await query.ToListAsync();
            var computerDtos = _mapper.Map<List<ComputerDto>>(computers);

            // Получаем список отделов для фильтрации
            var departments = await _context.Departments
                .Where(d => d.OfficeId == officeId)
                .ToListAsync();
            var departmentDtos = _mapper.Map<List<DepartmentDto>>(departments);

            // Передаем в представление
            ViewBag.Departments = new SelectList(departmentDtos, "Id", "Name");
            ViewBag.OfficeId = officeId; // Для использования в форме фильтрации

            return View(computerDtos);
        }



        // ИСПРАВЛЕННЫЙ метод Delete в ComputerController
        [HttpPost]
        [Route("Computer/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? departmentId = null)
        {
            var computer = await _context.Computers
                .Include(c => c.DisplayMonitors)
                .Include(c => c.Employee)
                    .ThenInclude(e => e.Department)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (computer == null)
            {
                return NotFound();
            }

            // Получаем departmentId из компьютера если не передан
            var redirectDepartmentId = departmentId ?? computer.Employee?.DepartmentId ?? 1;

            try
            {
                // Проверяем, есть ли связанные записи обслуживания
                var hasMaintenanceRecords = await _context.MaintenanceRecords.AnyAsync(mr => mr.ComputerId == id);
                if (hasMaintenanceRecords)
                {
                    TempData["Error"] = "Невозможно удалить компьютер, так как для него есть записи технического обслуживания.";
                    return RedirectToAction("Details", "Department", new { id = redirectDepartmentId });
                }

                // Проверяем, есть ли связанные заявки
                var hasServiceRequests = await _context.ServiceRequests.AnyAsync(sr => sr.ComputerId == id);
                if (hasServiceRequests)
                {
                    TempData["Error"] = "Невозможно удалить компьютер, так как для него есть заявки на обслуживание.";
                    return RedirectToAction("Details", "Department", new { id = redirectDepartmentId });
                }

                // Удаляем мониторы сначала
                _context.DisplayMonitors.RemoveRange(computer.DisplayMonitors);

                // Затем удаляем компьютер
                _context.Computers.Remove(computer);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Компьютер успешно удален.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при удалении компьютера: {ex.Message}";
            }

            return RedirectToAction("Details", "Department", new { id = redirectDepartmentId });
        }


        // GET: /Computer/Create
        public IActionResult Create(int employeeId)
        {
            var computerDto = new ComputerDto { EmployeeId = employeeId };
            return View(computerDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ComputerDto computerDto)
        {
            // Получаем выбранный OEM из формы
            var selectedOEM = Request.Form["OEMOption"].ToString();

            // ИСПРАВЛЕНИЕ: Проверяем и объединяем значения с проверкой на null
            if (!string.IsNullOrEmpty(selectedOEM))
            {
                computerDto.OSVersion = $"{(computerDto.OSVersion?.Trim() ?? string.Empty)} {selectedOEM}".Trim();
            }
            else
            {
                computerDto.OSVersion = computerDto.OSVersion?.Trim() ?? string.Empty;
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Ошибка: {error.ErrorMessage}");
                }
                return View(computerDto);
            }

            // Проверка существования сотрудника
            var employeeExists = await _context.Employees.AnyAsync(e => e.Id == computerDto.EmployeeId);
            if (!employeeExists)
            {
                ModelState.AddModelError(string.Empty, "Сотрудник не найден.");
                return View(computerDto);
            }

            var computer = new Computer
            {
                Model = computerDto.Model,
                Processor = computerDto.Processor,
                RAM = computerDto.RAM,
                OSVersion = computerDto.OSVersion,
                Storage = computerDto.Storage,
                EmployeeId = computerDto.EmployeeId,
                OSKey = computerDto.OSKey,
                OfficeKey = computerDto.OfficeKey,
                OfficeVersion = computerDto.OfficeVersion,
                UniqueIdentifier = GenerateUniqueIdentifier(computerDto.EmployeeId, DateTime.Now, computerDto.CustomIdentifierPart ?? "DEFAULT")
            };

            // Добавление мониторов
            if (computerDto.Monitors != null && computerDto.Monitors.Any())
            {
                foreach (var monitorDto in computerDto.Monitors)
                {
                    var monitor = new DisplayMonitor
                    {
                        Model = monitorDto.Model,
                        Resolution = monitorDto.Resolution,
                        Computer = computer
                    };
                    _context.DisplayMonitors.Add(monitor);
                }
            }

            _context.Computers.Add(computer);
            await _context.SaveChangesAsync();

            var employee = await _context.Employees.Include(e => e.Department)
                                                   .FirstOrDefaultAsync(e => e.Id == computerDto.EmployeeId);
            if (employee != null && employee.Department != null)
            {
                return RedirectToAction("Details", "Department", new { id = employee.DepartmentId });
            }

            return View(computerDto);
        }

        private string GenerateUniqueIdentifier(int employeeId, DateTime dateTime, string customPart)
        {
            var formattedDateTime = dateTime.ToString("yyyyMMdd");
            return $"{employeeId}-{formattedDateTime}-{customPart}";
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var computerDto = await _computerService.GetComputerByIdAsync(id);
            if (computerDto == null)
            {
                return NotFound();
            }

            return View(computerDto);
        }


        // Возможные значения OEM
        private readonly string[] possibleOEMs = new[] { "---", "OEM", "BOX" };

        // GET: /Computer/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var computer = await _context.Computers
                .Include(c => c.DisplayMonitors)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (computer == null)
            {
                return NotFound();
            }

            // Разделяем OSVersion на версию ОС и OEMOption
            string oemOption = string.Empty;
            string osVersion = computer.OSVersion;

            foreach (var oem in possibleOEMs)
            {
                if (!string.IsNullOrEmpty(computer.OSVersion) && computer.OSVersion.EndsWith(" " + oem))
                {
                    oemOption = oem;
                    osVersion = computer.OSVersion.Substring(0, computer.OSVersion.Length - oem.Length - 1).Trim();
                    break;
                }
            }

            var computerDto = new ComputerDto
            {
                Id = computer.Id,
                Model = computer.Model,
                Processor = computer.Processor,
                RAM = computer.RAM,
                OSVersion = osVersion,
                Storage = computer.Storage,
                EmployeeId = computer.EmployeeId,
                OSKey = computer.OSKey,
                OfficeKey = computer.OfficeKey,
                OfficeVersion = computer.OfficeVersion,
                Monitors = computer.DisplayMonitors.Select(m => new DisplayMonitorDto
                {
                    Id = m.Id,
                    Model = m.Model,
                    Resolution = m.Resolution
                }).ToList()
            };

            ViewBag.OEMOption = oemOption;
            ViewBag.PossibleOEMs = possibleOEMs;

            return View(computerDto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ComputerDto computerDto)
        {
            var selectedOEM = Request.Form["OEMOption"].ToString();

            if (!string.IsNullOrEmpty(selectedOEM))
            {
                computerDto.OSVersion = $"{computerDto.OSVersion.Trim()} {selectedOEM}".Trim();
            }
            else
            {
                computerDto.OSVersion = computerDto.OSVersion.Trim();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.OEMOption = selectedOEM;
                ViewBag.PossibleOEMs = possibleOEMs;

                // Вывод ошибок валидации
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }

                return View(computerDto);
            }

            var computer = await _context.Computers
                .Include(c => c.DisplayMonitors)
                .FirstOrDefaultAsync(c => c.Id == computerDto.Id);

            if (computer == null)
            {
                return NotFound();
            }

            computer.Model = computerDto.Model;
            computer.Processor = computerDto.Processor;
            computer.RAM = computerDto.RAM;
            computer.OSVersion = computerDto.OSVersion;
            computer.Storage = computerDto.Storage;
            computer.OSKey = computerDto.OSKey;
            computer.OfficeVersion = computerDto.OfficeVersion;
            computer.OfficeKey = computerDto.OfficeKey;

            // Обновление состояния объекта
            _context.Entry(computer).State = EntityState.Modified;

            // Удаление старых мониторов
            _context.DisplayMonitors.RemoveRange(computer.DisplayMonitors);

            if (computerDto.Monitors != null && computerDto.Monitors.Any())
            {
                foreach (var monitorDto in computerDto.Monitors)
                {
                    var monitor = new DisplayMonitor
                    {
                        Model = monitorDto.Model,
                        Resolution = monitorDto.Resolution,
                        Computer = computer
                    };
                    _context.DisplayMonitors.Add(monitor);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении изменений: {ex.Message}");
                ModelState.AddModelError("", "Произошла ошибка при сохранении данных.");
                return View(computerDto);
            }

            var employee = await _context.Employees.Include(e => e.Department)
                                                   .FirstOrDefaultAsync(e => e.Id == computerDto.EmployeeId);
            if (employee != null && employee.Department != null)
            {
                return RedirectToAction("Details", "Department", new { id = employee.DepartmentId });
            }

            // Если сотрудник или отдел не найдены, перенаправляем на другую страницу
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMonitor(int monitorId)
        {
            var monitor = await _context.DisplayMonitors.FindAsync(monitorId);
            if (monitor == null)
            {
                return Json(new { success = false, message = "Монитор не найден." });
            }

            _context.DisplayMonitors.Remove(monitor);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // GET: /Computer/Transfer/{id}
        [HttpGet]
        public async Task<IActionResult> Transfer(int id)
        {
            // Получаем компьютер и текущего сотрудника по ComputerId
            var computer = await _context.Computers
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (computer == null)
            {
                return NotFound();
            }

            // Заполняем DTO данными
            var transferDto = new TransferComputerDto
            {
                ComputerId = computer.Id,
                CurrentEmployeeId = computer.EmployeeId,
                CurrentEmployeeName = computer.Employee?.FullName ?? "Неизвестно"
            };

            // Получаем всех сотрудников для выбора нового владельца ПК
            ViewBag.Employees = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName");

            return View(transferDto);
        }

        // POST: /Computer/Transfer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(TransferComputerDto dto)
        {
            if (!ModelState.IsValid)
            {
                // Если модель не валидна, повторно загружаем список сотрудников и возвращаем форму
                ViewBag.Employees = new SelectList(await _context.Employees.ToListAsync(), "Id", "FullName");
                return View(dto);
            }

            // Ищем компьютер в базе данных
            var computer = await _context.Computers.FindAsync(dto.ComputerId);
            if (computer == null)
            {
                return NotFound();
            }

            // Обновляем владельца ПК
            computer.EmployeeId = dto.NewEmployeeId;

            // Сохраняем изменения в базе данных
            await _context.SaveChangesAsync();

            // Перенаправляем на страницу деталей компьютера или на другую страницу
            return RedirectToAction("Details", "Computer", new { id = dto.ComputerId });
        }

        [HttpGet]
        public async Task<IActionResult> GetKey(int computerId, string keyType)
        {
            
            var computer = await _context.Computers.FindAsync(computerId);
            if (computer == null)
            {
                return NotFound("Компьютер не найден.");
            }

            string keyValue = string.Empty;

            if (keyType == "OS")
            {
                keyValue = computer.OSKey;
            }
            else if (keyType == "Office")
            {
                keyValue = computer.OfficeKey;
            }
            else
            {
                return BadRequest("Неверный тип ключа.");
            }

            // Дополнительная проверка прав доступа (рекомендуется)
            // Например, убедитесь, что пользователь имеет право просматривать этот ключ

            if (string.IsNullOrEmpty(keyValue))
            {
                return NotFound("Ключ отсутствует.");
            }

            return Content(keyValue);
        }


        [HttpGet]
        public IActionResult ExportToExcel()
        {
            var computers = _context.Computers.Include(c => c.Employee)
                                              .ThenInclude(e => e.Department)
                                              .Include(c => c.DisplayMonitors)
                                              .ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Компьютеры");

                worksheet.Cell(1, 1).Value = "Модель";
                worksheet.Cell(1, 2).Value = "Процессор";
                worksheet.Cell(1, 3).Value = "ОС";
                worksheet.Cell(1, 4).Value = "RAM";
                worksheet.Cell(1, 5).Value = "Хранилище";
                worksheet.Cell(1, 6).Value = "Сотрудник";
                worksheet.Cell(1, 7).Value = "Отдел";
                worksheet.Cell(1, 8).Value = "Мониторы";

                var headerRange = worksheet.Range("A1:H1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                int row = 2;
                foreach (var computer in computers)
                {
                    worksheet.Cell(row, 1).Value = computer.Model;
                    worksheet.Cell(row, 2).Value = computer.Processor;
                    worksheet.Cell(row, 3).Value = computer.OSVersion;
                    worksheet.Cell(row, 4).Value = computer.RAM;
                    worksheet.Cell(row, 5).Value = computer.Storage;

                    if (computer.Employee != null)
                    {
                        worksheet.Cell(row, 6).Value = computer.Employee.FullName;
                        worksheet.Cell(row, 7).Value = computer.Employee.Department?.Name;
                    }

                    var monitorText = string.Join(", ", computer.DisplayMonitors.Select(m => $"{m.Model} ({m.Resolution})"));
                    worksheet.Cell(row, 8).Value = monitorText;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "computers.xlsx");
                }
            }
        }

        [HttpGet]
        public IActionResult ExportToPdf(int officeId)
        {
            // Фильтруем компьютеры по офису
            var computers = _context.Computers
                .Include(c => c.Employee)
                    .ThenInclude(e => e.Department)
                        .ThenInclude(d => d.Office)
                .Include(c => c.DisplayMonitors)
                .Where(c => c.Employee != null && c.Employee.Department.OfficeId == officeId)
                .ToList();

            if (!computers.Any())
            {
                return NotFound("В этом офисе нет компьютеров.");
            }

            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            page.Orientation = PageOrientation.Landscape;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Оптимизация шрифтов: увеличим шрифты для лучшей читабельности
            XFont titleFont = new XFont("Verdana", 14, XFontStyle.Bold); // Увеличиваем шрифт заголовка
            XFont headerFont = new XFont("Verdana", 10, XFontStyle.Bold); // Увеличиваем шрифт заголовков колонок
            XFont textFont = new XFont("Verdana", 8, XFontStyle.Regular); // Немного увеличим текст

            // Цвет границ
            XPen grayPen = new XPen(XColors.Gray, 0.5);

            int yPoint = 10;
            int xPoint = 10;
            int rowHeight = 10; // Увеличиваем высоту строки для лучшей читаемости

            // Ширины колонок: увеличим ширины, чтобы использовать больше места справа
            int modelColumnWidth = 180; // Увеличиваем ширину для модели
            int processorColumnWidth = 120; // Увеличиваем ширину для процессора
            int osColumnWidth = 100;
            int ramColumnWidth = 60;
            int storageColumnWidth = 120; // Увеличиваем ширину для хранилища
            int employeeColumnWidth = 150; // Увеличиваем ширину для сотрудника

            // Получаем название офиса (для заголовка)
            var officeName = computers.First().Employee?.Department?.Office?.Name ?? "Не указан офис";

            // Заголовок
            gfx.DrawString($" {officeName}", titleFont, XBrushes.Black, new XRect(xPoint, yPoint, page.Width.Point - 40, rowHeight), XStringFormats.TopLeft);
            yPoint += rowHeight + 5;

            yPoint += rowHeight;
            gfx.DrawLine(grayPen, xPoint, yPoint, page.Width.Point - xPoint, yPoint); // Линия под заголовками
            yPoint += 2;

            foreach (var computer in computers)
            {
                int requiredSpace = rowHeight * 5;

                // Проверка, нужно ли добавить новую страницу
                if (yPoint + requiredSpace > page.Height.Point - 40)
                {
                    page = document.AddPage();
                    page.Orientation = PageOrientation.Landscape;
                    gfx = XGraphics.FromPdfPage(page);
                    yPoint = 10;

                  
                    yPoint += rowHeight;
                    gfx.DrawLine(grayPen, xPoint, yPoint, page.Width.Point - xPoint, yPoint);
                    yPoint += 2;
                }

                // Заполнение данных компьютера
                gfx.DrawString(computer.Model ?? "N/A", textFont, XBrushes.Black, new XRect(xPoint, yPoint, modelColumnWidth, rowHeight), XStringFormats.TopLeft);
                gfx.DrawString(computer.Processor ?? "N/A", textFont, XBrushes.Black, new XRect(xPoint + modelColumnWidth, yPoint, processorColumnWidth, rowHeight), XStringFormats.TopLeft);
                gfx.DrawString(computer.OSVersion ?? "N/A", textFont, XBrushes.Black, new XRect(xPoint + modelColumnWidth + processorColumnWidth, yPoint, osColumnWidth, rowHeight), XStringFormats.TopLeft);
                gfx.DrawString(computer.RAM ?? "N/A", textFont, XBrushes.Black, new XRect(xPoint + modelColumnWidth + processorColumnWidth + osColumnWidth, yPoint, ramColumnWidth, rowHeight), XStringFormats.TopLeft);
                gfx.DrawString(computer.Storage ?? "N/A", textFont, XBrushes.Black, new XRect(xPoint + modelColumnWidth + processorColumnWidth + osColumnWidth + ramColumnWidth, yPoint, storageColumnWidth, rowHeight), XStringFormats.TopLeft);
                gfx.DrawString(computer.Employee?.FullName ?? "Не назначен", textFont, XBrushes.Black, new XRect(xPoint + modelColumnWidth + processorColumnWidth + osColumnWidth + ramColumnWidth + storageColumnWidth, yPoint, employeeColumnWidth, rowHeight), XStringFormats.TopLeft);

                yPoint += rowHeight;

                // Дополнительная информация
                gfx.DrawString($"Местоположение: {computer.Employee?.Department?.Name ?? "Не указан"}", textFont, XBrushes.Black, new XRect(xPoint + 10, yPoint, page.Width.Point - 60, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;
                gfx.DrawString($"Уникальный номер: {computer.UniqueIdentifier ?? "N/A"}", textFont, XBrushes.Black, new XRect(xPoint + 10, yPoint, page.Width.Point - 60, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;
                gfx.DrawString($"Операционная система: {computer.OSVersion ?? "N/A"}", textFont, XBrushes.Black, new XRect(xPoint + 10, yPoint, page.Width.Point - 60, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;
                gfx.DrawString($"Версия Оффис: {computer.OfficeVersion ?? "N/A"}", textFont, XBrushes.Black, new XRect(xPoint + 10, yPoint, page.Width.Point - 60, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                // Отображаем мониторы в одну строку для компактности
                var monitors = string.Join(", ", computer.DisplayMonitors.Select(m => $"{m.Model} ({m.Resolution})"));
                gfx.DrawString($"Мониторы: {monitors}", textFont, XBrushes.Black, new XRect(xPoint + 10, yPoint, page.Width.Point - 60, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                // Линия разделения
                gfx.DrawLine(grayPen, xPoint, yPoint, page.Width.Point - xPoint, yPoint);
                yPoint += 5; // Отступ после линии
            }

            // Сохранение документа в поток и возврат файла
            using (var stream = new MemoryStream())
            {
                document.Save(stream, false);
                return File(stream.ToArray(), "application/pdf", "computers_report.pdf");
            }
        }






        [HttpGet]
        public async Task<IActionResult> ExportComputerDetailsToPdf(int id)
        {
            var computerDto = await _computerService.GetComputerByIdAsync(id);
            if (computerDto == null)
            {
                return NotFound();
            }

            using (var stream = new MemoryStream())
            {
                var document = new PdfDocument();
                var page = document.AddPage();  // Ориентация по умолчанию вертикальная
                var gfx = XGraphics.FromPdfPage(page);

                var titleFont = new XFont("Arial", 20, XFontStyle.Bold);
                var headerFont = new XFont("Arial", 14, XFontStyle.Bold);
                var textFont = new XFont("Arial", 10, XFontStyle.Regular);
                var grayPen = new XPen(XColors.Gray, 1);

                double xPoint = 40;
                double yPoint = 40;
                double rowHeight = 20;
                double columnWidth = 300;  // Ширина для всех колонок

                // Заголовок
                gfx.DrawString($"Детали компьютера: {computerDto.UniqueIdentifier}", titleFont, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                // Основная информация о компьютере
                gfx.DrawString("Основная информация", headerFont, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                gfx.DrawString($"Модель: {computerDto.Model}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                gfx.DrawString($"Процессор: {computerDto.Processor}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                gfx.DrawString($"RAM: {computerDto.RAM}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                gfx.DrawString($"Хранилище: {computerDto.Storage}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                // Информация о сотруднике
                gfx.DrawString("Информация о сотруднике", headerFont, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                gfx.DrawString($"Сотрудник: {computerDto.Employee?.FullName ?? "Не назначен"}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                gfx.DrawString($"Отдел: {computerDto.Employee?.Department?.Name ?? "Не назначен"}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                gfx.DrawString($"Системная информация:", headerFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                gfx.DrawString($"Операционная система: {computerDto.OSVersion ?? "N/A"}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;
                gfx.DrawString($" - Ключ: {computerDto.OSKey ?? "N/A"}", textFont, XBrushes.Black, new XRect(xPoint + 10, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                gfx.DrawString($"Версия Оффис: {computerDto.OfficeVersion ?? "N/A"}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;
                gfx.DrawString($" - Ключ: {computerDto.OfficeKey ?? "N/A"}", textFont, XBrushes.Black, new XRect(xPoint + 10, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                // Информация о мониторах
                gfx.DrawString("Мониторы", headerFont, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                if (computerDto.Monitors != null && computerDto.Monitors.Any())
                {
                    foreach (var monitor in computerDto.Monitors)
                    {
                        gfx.DrawString($"Модель: {monitor.Model}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                        yPoint += rowHeight;

                        gfx.DrawString($"Разрешение: {monitor.Resolution}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                        yPoint += rowHeight;
                    }
                }
                else
                {
                    gfx.DrawString("Нет подключенных мониторов", textFont, XBrushes.Red, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                    yPoint += rowHeight;
                }

                // Граница внизу страницы
                gfx.DrawLine(grayPen, xPoint, yPoint, page.Width - 40, yPoint);

                document.Save(stream);
                var pdfContent = stream.ToArray();

                return File(pdfContent, "application/pdf", $"Computer_{computerDto.UniqueIdentifier}.pdf");
            }
        }


    }


}




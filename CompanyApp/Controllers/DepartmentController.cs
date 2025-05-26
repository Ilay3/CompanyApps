using CompanyApp.Application.DTOs;
using CompanyApp.Core.Interfaces;
using CompanyApp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace CompanyApp.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly IOfficeService _officeService;
        private readonly IBuildingService _buildingService;
        private readonly OfficeDbContext _context;

        public DepartmentController(IDepartmentService departmentService, IOfficeService officeService, IBuildingService buildingService, OfficeDbContext context)
        {
            _departmentService = departmentService;
            _officeService = officeService;
            _buildingService = buildingService;
            _context = context;
        }

        // GET: /Department/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: /Department/Create
        public async Task<IActionResult> Create(int officeId)
        {
            // Получаем здания, связанные с офисом
            var buildings = await _buildingService.GetBuildingsByOfficeIdAsync(officeId);

            // Передаем офис и здания в View
            ViewBag.OfficeId = officeId;
            ViewBag.Buildings = new SelectList(buildings, "Id", "Name");

            // Возвращаем представление
            return View();
        }

        // POST: /Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentDto dto)
        {
            if (ModelState.IsValid)
            {
                // Создаем новый отдел
                var createdDepartment = await _departmentService.CreateDepartmentAsync(dto);

                // После успешного создания перенаправляем на страницу с деталями отдела
                return RedirectToAction("Details", new { id = createdDepartment.Id });
            }

            // Если модель невалидна, загружаем корпуса для выбранного офиса
            var buildings = dto.OfficeId.HasValue
                ? await _buildingService.GetBuildingsByOfficeIdAsync(dto.OfficeId.Value)
                : new List<BuildingDto>();

            // Передаем офис и корпуса обратно в View
            ViewBag.OfficeId = dto.OfficeId;
            ViewBag.Buildings = new SelectList(buildings, "Id", "Name");

            return View(dto);
        }



        // AJAX Method to get buildings based on selected office
        public async Task<IActionResult> GetBuildings(int officeId)
        {
            var buildings = await _buildingService.GetBuildingsByOfficeIdAsync(officeId);

            if (buildings == null || !buildings.Any())
            {
                return Json(new List<BuildingDto>());
            }

            return Json(buildings.Select(b => new { id = b.Id, name = b.Name }));
        }


        // GET: /Department/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            // Получаем отдел по id
            var department = await _departmentService.GetDepartmentByIdAsync(id);

            if (department == null)
            {
                return NotFound();
            }

            // Получаем список офисов для dropdown
            var offices = await _officeService.GetAllOfficesAsync();
            ViewBag.Offices = new SelectList(offices, "Id", "Name", department.OfficeId);

            // Получаем список корпусов для dropdown
            var buildings = await _buildingService.GetBuildingsByOfficeIdAsync(department.OfficeId);
            ViewBag.Buildings = new SelectList(buildings, "Id", "Name", department.BuildingId);

            return View(department);
        }

        // POST: /Department/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepartmentDto departmentDto)
        {
            if (!ModelState.IsValid)
            {
                // Если модель невалидна, возвращаем данные обратно в представление
                var offices = await _officeService.GetAllOfficesAsync();
                ViewBag.Offices = new SelectList(offices, "Id", "Name", departmentDto.OfficeId);

                var buildings = await _buildingService.GetBuildingsByOfficeIdAsync(departmentDto.OfficeId);
                ViewBag.Buildings = new SelectList(buildings, "Id", "Name", departmentDto.BuildingId);

                return View(departmentDto);
            }

            // Обновляем отдел
            await _departmentService.UpdateDepartmentAsync(departmentDto);

            return RedirectToAction("Details", new { id = departmentDto.Id });
        }



        [HttpGet]
        public async Task<IActionResult> ExportDepartmentToPdf(int departmentId)
        {
            var department = await _context.Departments
                .Include(d => d.Equipments)
                .Include(d => d.Employees)
                    .ThenInclude(e => e.Computers)
                        .ThenInclude(c => c.DisplayMonitors)
                .FirstOrDefaultAsync(d => d.Id == departmentId);

            if (department == null)
            {
                return NotFound();
            }

            using (var stream = new MemoryStream())
            {
                var document = new PdfDocument();
                var page = document.AddPage();  // Вертикальная ориентация по умолчанию
                var gfx = XGraphics.FromPdfPage(page);

                var titleFont = new XFont("Arial", 20, XFontStyle.Bold);
                var headerFont = new XFont("Arial", 14, XFontStyle.Bold);
                var textFont = new XFont("Arial", 10, XFontStyle.Regular);
                var grayPen = new XPen(XColors.Gray, 1);

                double xPoint = 40;
                double yPoint = 40;
                double rowHeight = 20;
                double columnWidth = 300;  // Ширина для всех колонок
                double pageHeight = page.Height; // Высота страницы

                // Функция для добавления новой страницы, если место закончилось
                void CheckIfNewPageNeeded(ref PdfPage page, ref XGraphics gfx, ref double yPoint)
                {
                    if (yPoint + rowHeight > pageHeight - 40) // Если вышли за границы страницы
                    {
                        page = document.AddPage(); // Добавляем новую страницу
                        gfx = XGraphics.FromPdfPage(page); // Обновляем графический контекст
                        yPoint = 40; // Сбрасываем yPoint для новой страницы
                    }
                }

                // Заголовок
                gfx.DrawString($"Отдел: {department.Name}", titleFont, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, rowHeight), XStringFormats.TopLeft);
                yPoint += rowHeight;

                // Список сотрудников и их компьютеров
                if (department.Employees.Any())
                {
                    gfx.DrawString("Сотрудники и их компьютеры:", headerFont, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, rowHeight), XStringFormats.TopLeft);
                    yPoint += rowHeight;

                    foreach (var employee in department.Employees)
                    {
                        CheckIfNewPageNeeded(ref page, ref gfx, ref yPoint);
                        gfx.DrawString($"Сотрудник: {employee.FullName}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, page.Width, rowHeight), XStringFormats.TopLeft);
                        yPoint += rowHeight;

                        if (employee.Computers.Any())
                        {
                            foreach (var computer in employee.Computers)
                            {
                                CheckIfNewPageNeeded(ref page, ref gfx, ref yPoint);
                                gfx.DrawString($"Компьютер ID: {computer.UniqueIdentifier}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                                yPoint += rowHeight;

                                // Информация о компьютере
                                gfx.DrawString($"Модель: {computer.Model}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                                yPoint += rowHeight;

                                // Проверяем, нужно ли добавить новую страницу перед тем, как добавить мониторы
                                CheckIfNewPageNeeded(ref page, ref gfx, ref yPoint);
                                gfx.DrawString($"Процессор: {computer.Processor}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                                yPoint += rowHeight;
                                gfx.DrawString($"RAM: {computer.RAM}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                                yPoint += rowHeight;

                                gfx.DrawString($"Системная информация:", headerFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                                yPoint += rowHeight;

                                gfx.DrawString($"Операционная система: {computer.OSVersion ?? "N/A"}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                                yPoint += rowHeight;
                                gfx.DrawString($" - Ключ: {computer.OSKey ?? "N/A"}", textFont, XBrushes.Black, new XRect(xPoint + 10, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                                yPoint += rowHeight;

                                gfx.DrawString($"Версия Оффис: {computer.OfficeVersion ?? "N/A"}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                                yPoint += rowHeight;
                                gfx.DrawString($" - Ключ: {computer.OfficeKey ?? "N/A"}", textFont, XBrushes.Black, new XRect(xPoint + 10, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                                yPoint += rowHeight;

                                if (computer.DisplayMonitors.Any())
                                {
                                    CheckIfNewPageNeeded(ref page, ref gfx, ref yPoint);
                                    gfx.DrawString($"Мониторы:", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                                    yPoint += rowHeight;

                                    foreach (var monitor in computer.DisplayMonitors)
                                    {
                                        gfx.DrawString($"Модель: {monitor.Model}, Разрешение: {monitor.Resolution}", textFont, XBrushes.Black, new XRect(xPoint + 20, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                                        yPoint += rowHeight;
                                        CheckIfNewPageNeeded(ref page, ref gfx, ref yPoint);
                                    }
                                }
                                else
                                {
                                    gfx.DrawString($"Нет подключенных мониторов.", textFont, XBrushes.Red, new XRect(xPoint + 20, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                                    yPoint += rowHeight;
                                }

                                // Добавляем границу после каждого компьютера
                                gfx.DrawLine(grayPen, xPoint, yPoint, page.Width - 40, yPoint);
                                yPoint += 10;
                                CheckIfNewPageNeeded(ref page, ref gfx, ref yPoint);
                            }
                        }
                        else
                        {
                            gfx.DrawString("У сотрудника нет компьютеров.", textFont, XBrushes.Red, new XRect(xPoint + 20, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                            yPoint += rowHeight;
                            CheckIfNewPageNeeded(ref page, ref gfx, ref yPoint);
                        }
                    }
                }
                else
                {
                    gfx.DrawString("Нет сотрудников в данном отделе.", textFont, XBrushes.Red, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                    yPoint += rowHeight;
                }

                // Список оргтехники
                if (department.Equipments.Any())
                {
                    CheckIfNewPageNeeded(ref page, ref gfx, ref yPoint);
                    gfx.DrawString("Оргтехника:", headerFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                    yPoint += rowHeight;

                    foreach (var equipment in department.Equipments)
                    {
                        CheckIfNewPageNeeded(ref page, ref gfx, ref yPoint);
                        gfx.DrawString($"Тип: {equipment.Type}", textFont, XBrushes.Black, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                        gfx.DrawString($"Модель: {equipment.Model}", textFont, XBrushes.Black, new XRect(xPoint + 200, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                        yPoint += rowHeight;
                    }
                }
                else
                {
                    gfx.DrawString("Нет оргтехники в данном отделе.", textFont, XBrushes.Red, new XRect(xPoint, yPoint, columnWidth, rowHeight), XStringFormats.TopLeft);
                }

                document.Save(stream);
                var pdfContent = stream.ToArray();

                return File(pdfContent, "application/pdf", $"Department_{department.Name}.pdf");
            }
        }




    }

}

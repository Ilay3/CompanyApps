using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Application.InterfacesService;
using CompanyApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.IO;
using System.Drawing;
using System.Xml.Linq;
using PdfSharpCore;

namespace CompanyApp.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly OfficeDbContext _context;
        private readonly IMapper _mapper;

        public ReportService(OfficeDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<EquipmentReportDto> GenerateEquipmentReportAsync(DateTime startDate, DateTime endDate)
        {
            var report = new EquipmentReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Period = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}"
            };

            // Получаем перемещения компьютеров (из истории изменений)
            // Для этого нужно будет создать отдельную таблицу для отслеживания перемещений
            // Пока используем заглушку
            report.ComputerMovements = new List<ComputerMovementDto>();

            // Анализируем частые поломки
            var serviceRequests = await _context.ServiceRequests
                .Include(sr => sr.Computer)
                .Include(sr => sr.Equipment)
                .Include(sr => sr.StatusHistories)
                .Where(sr => sr.CreatedDate >= startDate && sr.CreatedDate <= endDate)
                .ToListAsync();

            // Группируем по оборудованию
            var computerBreakdowns = serviceRequests
                .Where(sr => sr.ComputerId.HasValue)
                .GroupBy(sr => sr.ComputerId)
                .Select(g => new
                {
                    ComputerId = g.Key,
                    Count = g.Count(),
                    Computer = g.First().Computer,
                    Issues = g.Select(sr => sr.Title).ToList()
                })
                .Where(x => x.Count > 1)
                .OrderByDescending(x => x.Count)
                .Take(10);

            report.FrequentBreakdowns = new List<FrequentBreakdownDto>();

            foreach (var breakdown in computerBreakdowns)
            {
                report.FrequentBreakdowns.Add(new FrequentBreakdownDto
                {
                    EquipmentType = "Компьютер",
                    EquipmentModel = breakdown.Computer?.Model ?? "Неизвестно",
                    EquipmentIdentifier = breakdown.Computer?.UniqueIdentifier ?? "Неизвестно",
                    BreakdownCount = breakdown.Count,
                    CommonIssues = breakdown.Issues.Distinct().ToList()
                });
            }

            // Анализируем причины поломок
            var breakdownReasons = serviceRequests
                .SelectMany(sr => sr.StatusHistories)
                .Where(sh => !string.IsNullOrEmpty(sh.Resolution))
                .GroupBy(sh => sh.Resolution)
                .Select(g => new BreakdownReasonDto
                {
                    Reason = g.Key,
                    Count = g.Count(),
                    Percentage = (g.Count() * 100.0m) / serviceRequests.Count
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            report.BreakdownReasons = breakdownReasons;

            // Общая статистика по заявкам
            report.ServiceRequestStatistics = new ServiceRequestStatisticsDto
            {
                TotalRequests = serviceRequests.Count,
                ResolvedRequests = serviceRequests.Count(sr => sr.Status == "Resolved" || sr.Status == "Closed"),
                PendingRequests = serviceRequests.Count(sr => sr.Status == "New" || sr.Status == "InProgress"),
                RequestsByType = serviceRequests.GroupBy(sr => sr.RequestType).ToDictionary(g => g.Key, g => g.Count()),
                RequestsByPriority = serviceRequests.GroupBy(sr => sr.Priority).ToDictionary(g => g.Key, g => g.Count())
            };

            // Рассчитываем среднее время решения
            var resolvedRequests = serviceRequests.Where(sr => sr.ResolvedDate.HasValue);
            if (resolvedRequests.Any())
            {
                report.ServiceRequestStatistics.AverageResolutionTime = resolvedRequests
                    .Average(sr => (sr.ResolvedDate.Value - sr.CreatedDate).TotalHours);
            }

            return report;
        }

        public async Task<byte[]> ExportReportToPdfAsync(EquipmentReportDto report)
        {
            using (var stream = new MemoryStream())
            {
                var document = new PdfDocument();
                var page = document.AddPage();
                page.Orientation = PageOrientation.Portrait;
                var gfx = XGraphics.FromPdfPage(page);

                var titleFont = new XFont("Arial", 20, XFontStyle.Bold);
                var headerFont = new XFont("Arial", 14, XFontStyle.Bold);
                var textFont = new XFont("Arial", 10, XFontStyle.Regular);

                double yPosition = 40;

                // Заголовок
                gfx.DrawString("Отчет по оборудованию", titleFont, XBrushes.Black,
                new XRect(40, yPosition, page.Width - 80, 30), XStringFormats.TopLeft);
                yPosition += 40;

                gfx.DrawString($"Период: {report.Period}", textFont, XBrushes.Black,
                    new XRect(40, yPosition, page.Width - 80, 20), XStringFormats.TopLeft);
                yPosition += 30;

                // Статистика по заявкам
                gfx.DrawString("Статистика по заявкам", headerFont, XBrushes.Black,
                new XRect(40, yPosition, page.Width - 80, 20), XStringFormats.TopLeft);
                yPosition += 25;

                gfx.DrawString($"Всего заявок: {report.ServiceRequestStatistics.TotalRequests}", textFont, XBrushes.Black,
                new XRect(40, yPosition, page.Width - 80, 20), XStringFormats.TopLeft);
                yPosition += 20;

                gfx.DrawString($"Решено: {report.ServiceRequestStatistics.ResolvedRequests}", textFont, XBrushes.Black,
                new XRect(40, yPosition, page.Width - 80, 20), XStringFormats.TopLeft);
                yPosition += 20;

                gfx.DrawString($"В работе: {report.ServiceRequestStatistics.PendingRequests}", textFont, XBrushes.Black,
                    new XRect(40, yPosition, page.Width - 80, 20), XStringFormats.TopLeft);
                yPosition += 30;

                // Частые поломки
                if (report.FrequentBreakdowns.Any())
                {
                    gfx.DrawString("Частые поломки", headerFont, XBrushes.Black,
                    new XRect(40, yPosition, page.Width - 80, 20), XStringFormats.TopLeft);
                    yPosition += 25;

                    foreach (var breakdown in report.FrequentBreakdowns.Take(5))
                    {
                        gfx.DrawString($"{breakdown.EquipmentModel} - {breakdown.BreakdownCount} поломок",
                            textFont, XBrushes.Black,
                            new XRect(40, yPosition, page.Width - 80, 20), XStringFormats.TopLeft);
                        yPosition += 20;
                    }
                }

                document.Save(stream);
                return stream.ToArray();
            }
        }

        public async Task<byte[]> ExportReportToExcelAsync(EquipmentReportDto report)
        {
            using (var workbook = new XLWorkbook())
            {
                // Общая статистика
                var summarySheet = workbook.Worksheets.Add("Общая статистика");
                summarySheet.Cell(1, 1).Value = "Отчет по оборудованию";
                summarySheet.Cell(2, 1).Value = $"Период: {report.Period}";

                summarySheet.Cell(4, 1).Value = "Показатель";
                summarySheet.Cell(4, 2).Value = "Значение";

                summarySheet.Cell(5, 1).Value = "Всего заявок";
                summarySheet.Cell(5, 2).Value = report.ServiceRequestStatistics.TotalRequests;

                summarySheet.Cell(6, 1).Value = "Решено";
                summarySheet.Cell(6, 2).Value = report.ServiceRequestStatistics.ResolvedRequests;

                summarySheet.Cell(7, 1).Value = "В работе";
                summarySheet.Cell(7, 2).Value = report.ServiceRequestStatistics.PendingRequests;

                // Частые поломки
                if (report.FrequentBreakdowns.Any())
                {
                    var breakdownsSheet = workbook.Worksheets.Add("Частые поломки");
                    breakdownsSheet.Cell(1, 1).Value = "Оборудование";
                    breakdownsSheet.Cell(1, 2).Value = "Модель";
                    breakdownsSheet.Cell(1, 3).Value = "Количество поломок";

                    int row = 2;
                    foreach (var breakdown in report.FrequentBreakdowns)
                    {
                        breakdownsSheet.Cell(row, 1).Value = breakdown.EquipmentType;
                        breakdownsSheet.Cell(row, 2).Value = breakdown.EquipmentModel;
                        breakdownsSheet.Cell(row, 3).Value = breakdown.BreakdownCount;
                        row++;
                    }
                }

                // Причины поломок
                if (report.BreakdownReasons.Any())
                {
                    var reasonsSheet = workbook.Worksheets.Add("Причины поломок");
                    reasonsSheet.Cell(1, 1).Value = "Причина";
                    reasonsSheet.Cell(1, 2).Value = "Количество";
                    reasonsSheet.Cell(1, 3).Value = "Процент";

                    int row = 2;
                    foreach (var reason in report.BreakdownReasons)
                    {
                        reasonsSheet.Cell(row, 1).Value = reason.Reason;
                        reasonsSheet.Cell(row, 2).Value = reason.Count;
                        reasonsSheet.Cell(row, 3).Value = $"{reason.Percentage:F2}%";
                        row++;
                    }
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
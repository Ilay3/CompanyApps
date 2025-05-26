using CompanyApp.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CompanyApp.Controllers
{
    [Authorize(Roles = "SysAdmin,Manager,Accountant")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: Report

        public IActionResult Index()
        {
            return View();
        }

        // POST: Report/Generate
        [HttpPost]
        public async Task<IActionResult> Generate(DateTime startDate, DateTime endDate)
        {
            var report = await _reportService.GenerateEquipmentReportAsync(startDate, endDate);
            return View("Report", report);
        }

        // GET: Report/ExportPdf
        public async Task<IActionResult> ExportPdf(DateTime startDate, DateTime endDate)
        {
            var report = await _reportService.GenerateEquipmentReportAsync(startDate, endDate);
            var pdfBytes = await _reportService.ExportReportToPdfAsync(report);

            return File(pdfBytes, "application/pdf", $"Equipment_Report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf");
        }

        // GET: Report/ExportExcel
        public async Task<IActionResult> ExportExcel(DateTime startDate, DateTime endDate)
        {
            var report = await _reportService.GenerateEquipmentReportAsync(startDate, endDate);
            var excelBytes = await _reportService.ExportReportToExcelAsync(report);

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Equipment_Report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx");
        }
    }
}
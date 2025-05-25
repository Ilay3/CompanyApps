using CompanyApp.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace CompanyApp.Application.InterfacesService
{
    public interface IReportService
    {
        Task<EquipmentReportDto> GenerateEquipmentReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> ExportReportToPdfAsync(EquipmentReportDto report);
        Task<byte[]> ExportReportToExcelAsync(EquipmentReportDto report);
    }
}
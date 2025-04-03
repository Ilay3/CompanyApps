using System.ComponentModel.DataAnnotations;

namespace CompanyApp.Application.DTOs
{
    public class ComputerDto
    {
        public int Id { get; set; }
        public string? UniqueIdentifier { get; set; }
        public string? Model { get; set; }
        public string? OSVersion { get; set; }
        public string? OSKey { get; set; } //Ключ для ОС
        public string? Processor { get; set; }
        public string? RAM { get; set; }
        public string? Storage { get; set; }
        public string? OfficeVersion { get; set; } // Версия MS Office
        public string? OfficeKey { get; set; } // Ключ для MS Office
        public int EmployeeId { get; set; }
        public EmployeeDto? Employee { get; set; } = new EmployeeDto();
        public List<DisplayMonitorDto> Monitors { get; set; } = new List<DisplayMonitorDto>();

        public string? CustomIdentifierPart { get; set; } // Номер

    }

}

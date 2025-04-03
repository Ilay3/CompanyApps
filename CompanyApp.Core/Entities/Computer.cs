using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Entities
{
    public class Computer
    {
        public int Id { get; set; }
        public string? UniqueIdentifier { get; set; } // Сгенерированный уникальный идентификатор
        public string? Model { get; set; }
        public string? OSVersion { get; set; }
        public string? OSKey { get; set; } // Новое поле: Ключ для ОС
        public string? Processor { get; set; }
        public string? RAM { get; set; }
        public string? Storage { get; set; }
        public string? OfficeVersion { get; set; } // Новое поле: Версия MS Office
        public string? OfficeKey { get; set; } // Новое поле: Ключ для MS Office
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public ICollection<DisplayMonitor>? DisplayMonitors { get; set; }
    }
}

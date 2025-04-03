using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? OfficeId { get; set; } // Отдел может находиться либо в офисе
        public Office? Office { get; set; }
        public int? BuildingId { get; set; } // либо в корпусе
        public Building? Building { get; set; }
        public ICollection<Employee>? Employees { get; set; }
        public ICollection<Equipment>? Equipments { get; set; }
    }
}

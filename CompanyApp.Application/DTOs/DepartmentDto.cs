using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.DTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int OfficeId { get; set; } 
        public Office? Office { get; set; }
        public int? BuildingId { get; set; } 
        public Building? Building { get; set; }
        public ICollection<Employee>? Employees { get; set; }
        public ICollection<Equipment>? Equipments { get; set; }

    }
}

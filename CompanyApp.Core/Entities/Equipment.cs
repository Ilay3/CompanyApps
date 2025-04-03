    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Entities
{
    public class Equipment
    {
        public int Id { get; set; }
        public string? Type { get; set; } // Тип техники (например, принтер, сканер)
        public string? Model { get; set; }
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}

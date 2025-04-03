using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.DTOs
{
        public class ChangeDepartmentDto
        {
            public int ComputerId { get; set; }
            public int? DepartmentId { get; set; }
            public int? EmployeeId { get; set; }

            public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
            public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
        }
}

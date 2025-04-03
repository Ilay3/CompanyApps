using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.DTOs
{
    public class OfficeDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public ICollection<BuildingDto>? Buildings { get; set; }
        public ICollection<DepartmentDto>? Departments { get; set; }
        public ICollection<DisplayMonitorDto>? Monitors { get; set; }

    }


}

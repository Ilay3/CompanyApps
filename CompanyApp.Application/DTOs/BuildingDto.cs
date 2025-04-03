using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.DTOs
{
    public class BuildingDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int OfficeId { get; internal set; }
        public IEnumerable<DepartmentDto> Departments { get; set; }
    }

}

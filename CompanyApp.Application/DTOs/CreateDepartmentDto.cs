using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.DTOs
{
    public class CreateDepartmentDto
    {
        public string Name { get; set; }
        public int? OfficeId { get; set; }
        public int? BuildingId { get; set; }
    }

}

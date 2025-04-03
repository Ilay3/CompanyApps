using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.DTOs
{
    public class CRUDEquipmentDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public int DepartmentId { get; set; }

    }

}
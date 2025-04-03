using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Entities
{
    public class Office
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public ICollection<Building>? Buildings { get; set; }
        public ICollection<Department>? Departments { get; set; }
    }
}

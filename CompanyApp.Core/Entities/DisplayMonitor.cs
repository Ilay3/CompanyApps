using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Entities
{
    public class DisplayMonitor
    {
        public int Id { get; set; }
        public string? Model { get; set; }
        public string? Resolution { get; set; }
        public int ComputerId { get; set; }
        public Computer? Computer { get; set; }
    }

}

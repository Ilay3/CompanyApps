using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.DTOs
{
    public class DisplayMonitorDto
    {
        public int Id { get; set; }

        public string? Model { get; set; }
        public string? Resolution { get; set; }

        public int ComputerId { get; set; } // Foreign key
        public Computer? Computer { get; set; }

    }
}
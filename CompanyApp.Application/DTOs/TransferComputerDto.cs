using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.DTOs
{
    public class TransferComputerDto
    {
        public int ComputerId { get; set; }

        public int CurrentEmployeeId { get; set; }
        public string? CurrentEmployeeName { get; set; }

        public int NewEmployeeId { get; set; } // Новый сотрудник для компьютера
    }

}

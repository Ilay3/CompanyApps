using CompanyApp.Application.DTOs;
using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Interfaces
{
    public interface IComputerService
    {
        Task<List<ComputerDto>> GetComputersByOfficeIdAsync(int officeId);
        Task<ComputerDto> GetComputerByIdAsync(int id);

        

    }
}

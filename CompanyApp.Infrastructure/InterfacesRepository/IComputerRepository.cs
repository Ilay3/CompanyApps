using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.InterfacesRepository
{
    public interface IComputerRepository
    {
        Task<List<Computer>> GetComputersByOfficeIdAsync(int officeId);

        Task<List<Employee>> GetEmployeesByDepartmentIdAsync(int departmentId);

        Task<Computer> GetComputerByIdAsync(int id);

    }
}

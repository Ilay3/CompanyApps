using CompanyApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentIdAsync(int departmentId);
        Task<EmployeeDto> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto> AddEmployeeAsync(EmployeeDto employeeDto);
        Task DeleteEmployeeAsync(int id);
    }

}

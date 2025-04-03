using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.InterfacesRepository
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetEmployeesByDepartmentIdAsync(int departmentId);
        Task<List<Department>> GetDepartmentsByOfficeIdAsync(int officeId);
        Task<Employee> GetEmployeeByIdAsync(int id);
        Task<Employee> AddEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(int id);


    }

}

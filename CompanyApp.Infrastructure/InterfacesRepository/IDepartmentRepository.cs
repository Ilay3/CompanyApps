using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.InterfacesRepository
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetDepartmentsByBuildingIdAsync(int buildingId);
        Task<IEnumerable<Department>> GetDepartmentsByOfficeIdAsync(int officeId);
        Task<Department> GetDepartmentByIdAsync(int id);
        Task<Department> CreateDepartmentAsync(Department department);
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();

        Task UpdateDepartmentAsync(Department department);
    }

}

using CompanyApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentDto>> GetDepartmentsByBuildingIdAsync(int buildingId);
        Task<IEnumerable<DepartmentDto>> GetDepartmentsByOfficeIdAsync(int officeId);

        Task<DepartmentDto> GetDepartmentByIdAsync(int id);
        Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto dto);


        Task UpdateDepartmentAsync(DepartmentDto departmentDto);

    }

}

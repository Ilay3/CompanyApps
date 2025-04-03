using CompanyApp.Infrastructure.InterfacesRepository;
using CompanyApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompanyApp.Infrastructure.Data;

namespace CompanyApp.Infrastructure.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly OfficeDbContext _context;

        public DepartmentRepository(OfficeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetDepartmentsByBuildingIdAsync(int buildingId)
        {
            return await _context.Departments
                .Where(d => d.BuildingId == buildingId)
                .Include(d => d.Employees)
                .Include(d => d.Equipments)
                .ToListAsync();
        }

        public async Task<IEnumerable<Department>> GetDepartmentsByOfficeIdAsync(int officeId)
        {
            return await _context.Departments
                .Where(d => d.OfficeId == officeId && !d.BuildingId.HasValue)
                .Include(d => d.Employees)
                .Include(d => d.Equipments)
                .ToListAsync();
        }

        public async Task<Department> GetDepartmentByIdAsync(int id)
        {
            return await _context.Departments
                    .Include(d => d.Equipments)
                    .Include(d => d.Employees) // Загружаем сотрудников
                        .ThenInclude(e => e.Computers) // Загружаем компьютеры сотрудников
                            .ThenInclude(c => c.DisplayMonitors) // Загружаем мониторы
                    .FirstOrDefaultAsync(d => d.Id == id);

        }
        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            return department;
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task UpdateDepartmentAsync(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }


    }
}

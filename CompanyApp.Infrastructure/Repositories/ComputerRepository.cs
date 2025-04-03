
using CompanyApp.Core.Entities;
using CompanyApp.Infrastructure.Data;
using CompanyApp.Infrastructure.InterfacesRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.Repositories
{
    public class ComputerRepository : IComputerRepository
    {
        private readonly OfficeDbContext _context;

        public ComputerRepository(OfficeDbContext context)
        {
            _context = context;
        }

        public async Task<List<Computer>> GetComputersByOfficeIdAsync(int officeId)
        {
            return await _context.Computers
                .Include(c => c.Employee)
                .ThenInclude(e => e.Department)
                .Include(c => c.DisplayMonitors)
                .Where(c => c.Employee.Department.OfficeId == officeId)
                .ToListAsync();
        }


        // Получаем список сотрудников по Id отдела
        public async Task<List<Employee>> GetEmployeesByDepartmentIdAsync(int departmentId)
        {
            return await _context.Employees
                                 .Where(e => e.DepartmentId == departmentId)
                                 .ToListAsync();
        }


        public async Task<Computer> GetComputerByIdAsync(int id)
        {
            return await _context.Computers
                .Include(c => c.Employee)
                .ThenInclude(e => e.Department) // Включаем отдел для сотрудника
                .Include(c => c.DisplayMonitors) // Включаем мониторы
                .FirstOrDefaultAsync(c => c.Id == id);
        }

    }
}

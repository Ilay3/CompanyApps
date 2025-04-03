using CompanyApp.Infrastructure.InterfacesRepository;
using CompanyApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompanyApp.Infrastructure.Data;

namespace CompanyApp.Application.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly OfficeDbContext _context;

        public EmployeeRepository(OfficeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByDepartmentIdAsync(int departmentId)
        {
            return await _context.Employees
                .Where(e => e.DepartmentId == departmentId)
                .ToListAsync();
        }

        public async Task<List<Department>> GetDepartmentsByOfficeIdAsync(int officeId)
        {
            return await _context.Departments
                .Where(d => d.OfficeId == officeId)
                .Include(d => d.Employees)
                .ToListAsync();
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees
                .FindAsync(id);
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }


        public async Task DeleteEmployeeAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }
    }
}

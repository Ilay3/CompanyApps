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
    public class OfficeRepository : IOfficeRepository
    {
        private readonly OfficeDbContext _context;

        public OfficeRepository(OfficeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Office>> GetAllOfficesAsync()
        {
            return await _context.Offices.Include(o => o.Buildings).Include(o => o.Departments).ToListAsync();
        }

        public async Task<Office> GetOfficeByIdAsync(int id)
        {
            return await _context.Offices
                .Include(o => o.Buildings)
                .Include(o => o.Departments)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<int> CreateOfficeAsync(Office office)
        {
            _context.Offices.Add(office);
            await _context.SaveChangesAsync();
            return office.Id;
        }

        public async Task UpdateOfficeAsync(Office office)
        {
            _context.Offices.Update(office);
            await _context.SaveChangesAsync();
        }

    }

}

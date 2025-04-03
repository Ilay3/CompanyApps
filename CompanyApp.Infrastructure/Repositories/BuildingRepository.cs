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
    public class BuildingRepository : IBuildingRepository
    {
        private readonly OfficeDbContext _context;

        public BuildingRepository(OfficeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Building>> GetBuildingsByOfficeIdAsync(int officeId)
        {
            return await _context.Buildings
                .Where(b => b.OfficeId == officeId)
                .Include(b => b.Departments)
                .ToListAsync();
        }

        public async Task<Building> GetBuildingByIdAsync(int id)
        {
            return await _context.Buildings
                .Include(b => b.Departments)
                .FirstOrDefaultAsync(b => b.Id == id);
        }


    }
}

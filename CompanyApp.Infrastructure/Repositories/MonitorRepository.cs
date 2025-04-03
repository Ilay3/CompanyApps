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
    public class DisplayMonitorRepository : IDisplayMonitorRepository
    {
        private readonly OfficeDbContext _context;

        public DisplayMonitorRepository(OfficeDbContext context)
        {
            _context = context;
        }

        public IEnumerable<DisplayMonitor> GetByComputerId(int computerId)
        {
            return _context.Set<DisplayMonitor>().Where(m => m.ComputerId == computerId).ToList();
        }

        public DisplayMonitor GetById(int id)
        {
            return _context.Set<DisplayMonitor>().Find(id);
        }

        public void Add(DisplayMonitor DisplayMonitor)
        {
            _context.Set<DisplayMonitor>().Add(DisplayMonitor);
            _context.SaveChanges();
        }

        public void Update(DisplayMonitor DisplayMonitor)
        {
            _context.Set<DisplayMonitor>().Update(DisplayMonitor);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var DisplayMonitor = _context.Set<DisplayMonitor>().Find(id);
            if (DisplayMonitor != null)
            {
                _context.Set<DisplayMonitor>().Remove(DisplayMonitor);
                _context.SaveChanges();
            }
        }

        public async Task DeleteAsync(DisplayMonitor monitor)
        {
            _context.DisplayMonitors.Remove(monitor);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }

}

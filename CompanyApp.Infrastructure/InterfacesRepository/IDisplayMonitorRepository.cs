using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.InterfacesRepository
{
    public interface IDisplayMonitorRepository
    {
        IEnumerable<DisplayMonitor> GetByComputerId(int computerId);
        DisplayMonitor GetById(int id);
        void Add(DisplayMonitor DisplayMonitor);
        void Update(DisplayMonitor DisplayMonitor);
        Task DeleteAsync(DisplayMonitor monitor);
        Task SaveChangesAsync();

    }
}

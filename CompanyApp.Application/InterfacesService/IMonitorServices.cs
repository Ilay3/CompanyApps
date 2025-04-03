using CompanyApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Interfaces
{
    public interface IMonitorService
    {
        IEnumerable<DisplayMonitorDto> GetMonitorsByComputerId(int computerId);
        DisplayMonitorDto GetMonitorById(int id);
        void AddMonitor(DisplayMonitorDto monitorDto);
    }
}

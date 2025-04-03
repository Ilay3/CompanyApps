using CompanyApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.InterfacesService
{
    public interface ISoftwareLicenseService
    {
        Task<IEnumerable<SoftwareLicenseDto>> GetAllSoftwareLicensesAsync();
        Task<SoftwareLicenseDto> GetSoftwareLicenseByIdAsync(int id);
        Task<SoftwareLicenseDto> CreateSoftwareLicenseAsync(SoftwareLicenseDto softwareLicenseDto);
        Task UpdateSoftwareLicenseAsync(SoftwareLicenseDto softwareLicenseDto);
        Task DeleteSoftwareLicenseAsync(int id);
        Task<IEnumerable<SoftwareLicenseDto>> GetSoftwareLicensesByComputerIdAsync(int computerId);
        Task AssignSoftwareToComputersAsync(AssignSoftwareDto assignSoftwareDto);
        Task UnassignSoftwareFromComputerAsync(int computerId, int softwareLicenseId);
        Task<IEnumerable<SoftwareLicenseDto>> GetExpiringSoftwareLicensesAsync(int daysThreshold);
    }

}

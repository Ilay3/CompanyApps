using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.InterfacesRepository
{
    public interface ISoftwareLicenseRepository
    {
        Task<IEnumerable<SoftwareLicense>> GetAllSoftwareLicensesAsync();
        Task<SoftwareLicense> GetSoftwareLicenseByIdAsync(int id);
        Task<SoftwareLicense> CreateSoftwareLicenseAsync(SoftwareLicense softwareLicense);
        Task UpdateSoftwareLicenseAsync(SoftwareLicense softwareLicense);
        Task DeleteSoftwareLicenseAsync(int id);
        Task<IEnumerable<SoftwareLicense>> GetSoftwareLicensesByComputerIdAsync(int computerId);
        Task<ComputerSoftware> AssignSoftwareToComputerAsync(ComputerSoftware computerSoftware);
        Task UnassignSoftwareFromComputerAsync(int computerId, int softwareLicenseId);
        Task<IEnumerable<SoftwareLicense>> GetExpiringSoftwareLicensesAsync(DateTime expirationThreshold);
    }

}

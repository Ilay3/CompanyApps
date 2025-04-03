using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.InterfacesRepository
{
    public interface IOfficeRepository
    {
        Task<IEnumerable<Office>> GetAllOfficesAsync();
        Task<Office> GetOfficeByIdAsync(int id);

        Task<int> CreateOfficeAsync(Office office);

        Task UpdateOfficeAsync(Office office);

    }
}

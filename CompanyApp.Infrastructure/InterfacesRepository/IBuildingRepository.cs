using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.InterfacesRepository
{
    public interface IBuildingRepository
    {
        Task<IEnumerable<Building>> GetBuildingsByOfficeIdAsync(int officeId);
        Task<Building> GetBuildingByIdAsync(int id);


    }
}

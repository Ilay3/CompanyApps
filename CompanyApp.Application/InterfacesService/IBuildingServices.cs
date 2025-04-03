using CompanyApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Interfaces
{
    public interface IBuildingService
    {
        Task<IEnumerable<BuildingDto>> GetBuildingsByOfficeIdAsync(int officeId);
        Task<BuildingDto> GetBuildingByIdAsync(int id);

    }
}


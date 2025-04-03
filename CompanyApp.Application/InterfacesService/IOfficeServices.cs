using CompanyApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Interfaces
{
    public interface IOfficeService
    {
        Task<IEnumerable<OfficeDto>> GetAllOfficesAsync();
        Task<OfficeDto> GetOfficeByIdAsync(int id);

        Task<int> CreateOfficeAsync(OfficeDto officeDto);
        Task UpdateOfficeAsync(OfficeDto officeDto);
    }

}

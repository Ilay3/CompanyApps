using CompanyApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Interfaces
{
    public interface IEquipmentService
    {
        Task<EquipmentDto> CreateEquipmentAsync(CRUDEquipmentDto dto);
        Task<EquipmentDto> UpdateEquipmentAsync(CRUDEquipmentDto dto); 
        Task DeleteEquipmentAsync(int id);
        Task<CRUDEquipmentDto> GetEquipmentByIdAsync(int id);

        Task<IEnumerable<CRUDEquipmentDto>> GetAllEquipmentsAsync();

    }
}

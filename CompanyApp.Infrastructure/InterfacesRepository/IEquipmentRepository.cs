using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.InterfacesRepository
{
    public interface IEquipmentRepository
    {
        Task<Equipment> CreateEquipmentAsync(Equipment equipment);
        Task<Equipment> UpdateEquipmentAsync(Equipment equipment);
        Task DeleteEquipmentAsync(int id);
        Task<Equipment> GetEquipmentByIdAsync(int id); 


    }
}

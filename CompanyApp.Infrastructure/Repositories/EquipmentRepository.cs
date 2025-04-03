using CompanyApp.Infrastructure.InterfacesRepository;
using CompanyApp.Core.Entities;
using CompanyApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.Repositories
{
    public class EquipmentRepository : IEquipmentRepository
    {
        private readonly OfficeDbContext _context;

        public EquipmentRepository(OfficeDbContext context)
        {
            _context = context;
        }

        public async Task<Equipment> CreateEquipmentAsync(Equipment equipment)
        {
            _context.Equipments.Add(equipment);
            await _context.SaveChangesAsync();
            return equipment;
        }
        public async Task<Equipment> UpdateEquipmentAsync(Equipment equipment)
        {
            _context.Equipments.Update(equipment);
            await _context.SaveChangesAsync();
            return equipment;
        }

        public async Task DeleteEquipmentAsync(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment != null)
            {
                _context.Equipments.Remove(equipment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Equipment> GetEquipmentByIdAsync(int id)
        {
            return await _context.Equipments.FindAsync(id);
        }

    }

}

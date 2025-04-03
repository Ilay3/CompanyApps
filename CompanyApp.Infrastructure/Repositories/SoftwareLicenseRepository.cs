using CompanyApp.Core.Entities;
using CompanyApp.Infrastructure.Data;
using CompanyApp.Infrastructure.InterfacesRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Infrastructure.Repositories
{
    public class SoftwareLicenseRepository : ISoftwareLicenseRepository
    {
        private readonly OfficeDbContext _context;

        public SoftwareLicenseRepository(OfficeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SoftwareLicense>> GetAllSoftwareLicensesAsync()
        {
            return await _context.SoftwareLicenses
                .Include(s => s.ComputerSoftware)
                    .ThenInclude(cs => cs.Computer)
                .ToListAsync();
        }

        public async Task<SoftwareLicense> GetSoftwareLicenseByIdAsync(int id)
        {
            return await _context.SoftwareLicenses
                .Include(s => s.ComputerSoftware)
                    .ThenInclude(cs => cs.Computer)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<SoftwareLicense> CreateSoftwareLicenseAsync(SoftwareLicense softwareLicense)
        {
            _context.SoftwareLicenses.Add(softwareLicense);
            await _context.SaveChangesAsync();
            return softwareLicense;
        }

        public async Task UpdateSoftwareLicenseAsync(SoftwareLicense softwareLicense)
        {
            _context.Entry(softwareLicense).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSoftwareLicenseAsync(int id)
        {
            var softwareLicense = await _context.SoftwareLicenses.FindAsync(id);
            if (softwareLicense != null)
            {
                // Сначала удаляем все связи с компьютерами
                var computerSoftware = await _context.ComputerSoftware
                    .Where(cs => cs.SoftwareLicenseId == id)
                    .ToListAsync();

                _context.ComputerSoftware.RemoveRange(computerSoftware);

                // Затем удаляем саму лицензию
                _context.SoftwareLicenses.Remove(softwareLicense);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<SoftwareLicense>> GetSoftwareLicensesByComputerIdAsync(int computerId)
        {
            return await _context.ComputerSoftware
                .Where(cs => cs.ComputerId == computerId)
                .Select(cs => cs.SoftwareLicense)
                .ToListAsync();
        }

        public async Task<ComputerSoftware> AssignSoftwareToComputerAsync(ComputerSoftware computerSoftware)
        {
            // Проверяем, не превышено ли количество лицензий
            var softwareLicense = await _context.SoftwareLicenses.FindAsync(computerSoftware.SoftwareLicenseId);

            if (softwareLicense != null)
            {
                var usedLicenses = await _context.ComputerSoftware
                    .CountAsync(cs => cs.SoftwareLicenseId == computerSoftware.SoftwareLicenseId);

                if (usedLicenses >= softwareLicense.Seats)
                {
                    throw new InvalidOperationException("Превышено максимальное количество лицензий");
                }

                // Проверяем, не установлено ли уже ПО на этот компьютер
                var existingInstallation = await _context.ComputerSoftware
                    .FirstOrDefaultAsync(cs => cs.ComputerId == computerSoftware.ComputerId &&
                                              cs.SoftwareLicenseId == computerSoftware.SoftwareLicenseId);

                if (existingInstallation != null)
                {
                    throw new InvalidOperationException("ПО уже установлено на этот компьютер");
                }

                // Добавляем новую запись
                _context.ComputerSoftware.Add(computerSoftware);

                // Обновляем количество использованных лицензий
                softwareLicense.SeatsUsed = usedLicenses + 1;
                _context.Entry(softwareLicense).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return computerSoftware;
            }

            throw new InvalidOperationException("Лицензия не найдена");
        }

        public async Task UnassignSoftwareFromComputerAsync(int computerId, int softwareLicenseId)
        {
            var computerSoftware = await _context.ComputerSoftware
                .FirstOrDefaultAsync(cs => cs.ComputerId == computerId &&
                                          cs.SoftwareLicenseId == softwareLicenseId);

            if (computerSoftware != null)
            {
                // Удаляем запись
                _context.ComputerSoftware.Remove(computerSoftware);

                // Обновляем количество использованных лицензий
                var softwareLicense = await _context.SoftwareLicenses.FindAsync(softwareLicenseId);
                if (softwareLicense != null && softwareLicense.SeatsUsed > 0)
                {
                    softwareLicense.SeatsUsed--;
                    _context.Entry(softwareLicense).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<SoftwareLicense>> GetExpiringSoftwareLicensesAsync(DateTime expirationThreshold)
        {
            return await _context.SoftwareLicenses
                .Where(s => s.ExpirationDate.HasValue && s.ExpirationDate <= expirationThreshold)
                .Include(s => s.ComputerSoftware)
                    .ThenInclude(cs => cs.Computer)
                .ToListAsync();
        }
    }
}
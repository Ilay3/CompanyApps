using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Application.InterfacesService;
using CompanyApp.Core.Entities;
using CompanyApp.Core.Interfaces;
using CompanyApp.Infrastructure.InterfacesRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.Services
{
    public class SoftwareLicenseService : ISoftwareLicenseService
    {
        private readonly ISoftwareLicenseRepository _softwareLicenseRepository;
        private readonly IMapper _mapper;

        public SoftwareLicenseService(ISoftwareLicenseRepository softwareLicenseRepository, IMapper mapper)
        {
            _softwareLicenseRepository = softwareLicenseRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SoftwareLicenseDto>> GetAllSoftwareLicensesAsync()
        {
            var softwareLicenses = await _softwareLicenseRepository.GetAllSoftwareLicensesAsync();
            return _mapper.Map<IEnumerable<SoftwareLicenseDto>>(softwareLicenses);
        }

        public async Task<SoftwareLicenseDto> GetSoftwareLicenseByIdAsync(int id)
        {
            var softwareLicense = await _softwareLicenseRepository.GetSoftwareLicenseByIdAsync(id);
            return _mapper.Map<SoftwareLicenseDto>(softwareLicense);
        }

        public async Task<SoftwareLicenseDto> CreateSoftwareLicenseAsync(SoftwareLicenseDto softwareLicenseDto)
        {
            var softwareLicense = _mapper.Map<SoftwareLicense>(softwareLicenseDto);
            softwareLicense = await _softwareLicenseRepository.CreateSoftwareLicenseAsync(softwareLicense);
            return _mapper.Map<SoftwareLicenseDto>(softwareLicense);
        }

        public async Task UpdateSoftwareLicenseAsync(SoftwareLicenseDto softwareLicenseDto)
        {
            var softwareLicense = _mapper.Map<SoftwareLicense>(softwareLicenseDto);
            await _softwareLicenseRepository.UpdateSoftwareLicenseAsync(softwareLicense);
        }

        public async Task DeleteSoftwareLicenseAsync(int id)
        {
            await _softwareLicenseRepository.DeleteSoftwareLicenseAsync(id);
        }

        public async Task<IEnumerable<SoftwareLicenseDto>> GetSoftwareLicensesByComputerIdAsync(int computerId)
        {
            var softwareLicenses = await _softwareLicenseRepository.GetSoftwareLicensesByComputerIdAsync(computerId);
            return _mapper.Map<IEnumerable<SoftwareLicenseDto>>(softwareLicenses);
        }

        public async Task AssignSoftwareToComputersAsync(AssignSoftwareDto assignSoftwareDto)
        {
            foreach (var computerId in assignSoftwareDto.ComputerIds)
            {
                var computerSoftware = new ComputerSoftware
                {
                    ComputerId = computerId,
                    SoftwareLicenseId = assignSoftwareDto.SoftwareLicenseId,
                    InstallationDate = assignSoftwareDto.InstallationDate,
                    Notes = assignSoftwareDto.Notes
                };

                await _softwareLicenseRepository.AssignSoftwareToComputerAsync(computerSoftware);
            }
        }

        public async Task UnassignSoftwareFromComputerAsync(int computerId, int softwareLicenseId)
        {
            await _softwareLicenseRepository.UnassignSoftwareFromComputerAsync(computerId, softwareLicenseId);
        }

        public async Task<IEnumerable<SoftwareLicenseDto>> GetExpiringSoftwareLicensesAsync(int daysThreshold)
        {
            var expirationThreshold = DateTime.Now.AddDays(daysThreshold);
            var expiringSoftware = await _softwareLicenseRepository.GetExpiringSoftwareLicensesAsync(expirationThreshold);
            return _mapper.Map<IEnumerable<SoftwareLicenseDto>>(expiringSoftware);
        }
    }
}
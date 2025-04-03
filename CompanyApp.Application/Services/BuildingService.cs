using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Core.Entities;
using CompanyApp.Core.Interfaces;
using CompanyApp.Infrastructure.InterfacesRepository;
using CompanyApp.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.Services
{
    public class BuildingService : IBuildingService
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IMapper _mapper;

        public BuildingService(IBuildingRepository buildingRepository, IMapper mapper)
        {
            _buildingRepository = buildingRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BuildingDto>> GetBuildingsByOfficeIdAsync(int officeId)
        {
            var buildings = await _buildingRepository.GetBuildingsByOfficeIdAsync(officeId);
            return _mapper.Map<IEnumerable<BuildingDto>>(buildings);
        }

        public async Task<BuildingDto> GetBuildingByIdAsync(int id)
        {
            var building = await _buildingRepository.GetBuildingByIdAsync(id);
            return _mapper.Map<BuildingDto>(building);
        }
    }
}

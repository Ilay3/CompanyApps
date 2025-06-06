using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Infrastructure.InterfacesRepository;
using CompanyApp.Core.Entities;
using CompanyApp.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.Services
{
    public class EquipmentService : IEquipmentService
    {
        private readonly IEquipmentRepository _equipmentRepository;
        private readonly IMapper _mapper;

        public EquipmentService(IEquipmentRepository equipmentRepository, IMapper mapper)
        {
            _equipmentRepository = equipmentRepository;
            _mapper = mapper;
        }
        public async Task<EquipmentDto> CreateEquipmentAsync(CRUDEquipmentDto dto)
        {
            var equipment = _mapper.Map<Equipment>(dto);
            equipment = await _equipmentRepository.CreateEquipmentAsync(equipment);
            return _mapper.Map<EquipmentDto>(equipment);
        }

        public async Task<EquipmentDto> UpdateEquipmentAsync(CRUDEquipmentDto dto)
        {
            var equipment = _mapper.Map<Equipment>(dto);
            equipment = await _equipmentRepository.UpdateEquipmentAsync(equipment);
            return _mapper.Map<EquipmentDto>(equipment);
        }


        public async Task DeleteEquipmentAsync(int id)
        {
            await _equipmentRepository.DeleteEquipmentAsync(id);
        }

        public async Task<CRUDEquipmentDto> GetEquipmentByIdAsync(int id)
        {
            var equipment = await _equipmentRepository.GetEquipmentByIdAsync(id);
            return _mapper.Map<CRUDEquipmentDto>(equipment);
        }

        public async Task<IEnumerable<CRUDEquipmentDto>> GetAllEquipmentsAsync()
        {
            var equipments = await _equipmentRepository.GetAllEquipmentsAsync();
            return _mapper.Map<IEnumerable<CRUDEquipmentDto>>(equipments);
        }
    }

}

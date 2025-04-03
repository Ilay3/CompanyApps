using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Infrastructure.InterfacesRepository;
using CompanyApp.Core.Entities;
using CompanyApp.Core.Interfaces;


namespace CompanyApp.Application.Services
{
    public class OfficeService : IOfficeService
    {
        private readonly IOfficeRepository _officeRepository;
        private readonly IMapper _mapper;

        public OfficeService(IOfficeRepository officeRepository, IMapper mapper)
        {
            _officeRepository = officeRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OfficeDto>> GetAllOfficesAsync()
        {
            var offices = await _officeRepository.GetAllOfficesAsync();
            return _mapper.Map<IEnumerable<OfficeDto>>(offices);
        }

        public async Task<OfficeDto> GetOfficeByIdAsync(int id)
        {
            var office = await _officeRepository.GetOfficeByIdAsync(id);
            return _mapper.Map<OfficeDto>(office);
        }

        public async Task<int> CreateOfficeAsync(OfficeDto officeDto)
        {
            // Создаем новый офис на основе DTO
            var office = new Office
            {
                Name = officeDto.Name,
                City = officeDto.City
            };

            return await _officeRepository.CreateOfficeAsync(office);
        }

        public async Task UpdateOfficeAsync(OfficeDto officeDto)
        {
            var office = await _officeRepository.GetOfficeByIdAsync(officeDto.Id);
            if (office == null)
            {
                throw new Exception("Office not found");
            }

            office.Name = officeDto.Name;
            office.City = officeDto.City;

            await _officeRepository.UpdateOfficeAsync(office);
        }

    }
}

using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Infrastructure.InterfacesRepository;
using CompanyApp.Core.Interfaces;

namespace CompanyApp.Application.Services
{
    public class ComputerService : IComputerService
    {
        private readonly IComputerRepository _computerRepository;
        private readonly IMapper _mapper;

        public ComputerService(IComputerRepository computerRepository, IMapper mapper)
        {
            _computerRepository = computerRepository;
            _mapper = mapper;
        }

        public async Task<List<ComputerDto>> GetComputersByOfficeIdAsync(int officeId)
        {
            var computers = await _computerRepository.GetComputersByOfficeIdAsync(officeId);
            return _mapper.Map<List<ComputerDto>>(computers);
        }

        public async Task<ComputerDto> GetComputerByIdAsync(int id)
        {
            var computer = await _computerRepository.GetComputerByIdAsync(id);
            if (computer == null)
            {
                return null;
            }

            // Ручной маппинг
            var computerDto = new ComputerDto
            {
                Id = computer.Id,
                UniqueIdentifier = computer.UniqueIdentifier,
                Model = computer.Model,
                OSVersion = computer.OSVersion,
                Processor = computer.Processor,
                RAM = computer.RAM,
                Storage = computer.Storage,
                EmployeeId = computer.EmployeeId,
                OSKey = computer.OSKey,
                OfficeVersion = computer.OfficeVersion,
                OfficeKey = computer.OfficeKey,
                Employee = new EmployeeDto
                {
                    Id = computer.Employee?.Id ?? 0,
                    FullName = computer.Employee?.FullName,
                    DepartmentId = computer.Employee?.DepartmentId ?? 0,
                    Department = computer.Employee?.Department
                },
                Monitors = computer.DisplayMonitors?.Select(m => new DisplayMonitorDto
                {
                    Id = m.Id,
                    Model = m.Model,
                    Resolution = m.Resolution
                }).ToList() ?? new List<DisplayMonitorDto>()
            };

            // Логируем результат
            Console.WriteLine($"Мониторы в ручном маппинге: {computerDto.Monitors.Count}");

            return computerDto;
        }

    }
}

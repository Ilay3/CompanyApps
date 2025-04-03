using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Infrastructure.InterfacesRepository;
using CompanyApp.Core.Entities;
using CompanyApp.Core.Interfaces;

namespace CompanyApp.Application.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMapper _mapper;

        public DepartmentService(IDepartmentRepository departmentRepository, IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<DepartmentDto>> GetDepartmentsByBuildingIdAsync(int buildingId)
        {
            var departments = await _departmentRepository.GetDepartmentsByBuildingIdAsync(buildingId);
            return _mapper.Map<IEnumerable<DepartmentDto>>(departments);
        }

        public async Task<IEnumerable<DepartmentDto>> GetDepartmentsByOfficeIdAsync(int officeId)
        {
            var departments = await _departmentRepository.GetDepartmentsByOfficeIdAsync(officeId);
            return _mapper.Map<IEnumerable<DepartmentDto>>(departments);
        }

        public async Task<DepartmentDto> GetDepartmentByIdAsync(int id)
        {
            var department = await _departmentRepository.GetDepartmentByIdAsync(id);
            return _mapper.Map<DepartmentDto>(department);
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto dto)
        {
            var department = _mapper.Map<Department>(dto);
            department = await _departmentRepository.CreateDepartmentAsync(department);
            return _mapper.Map<DepartmentDto>(department);
        }

        public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetAllDepartmentsAsync(); 
            return _mapper.Map<IEnumerable<DepartmentDto>>(departments);
        }


        public async Task UpdateDepartmentAsync(DepartmentDto departmentDto)
        {
            var department = await _departmentRepository.GetDepartmentByIdAsync(departmentDto.Id);

            if (department != null)
            {
                department.Name = departmentDto.Name;
                department.OfficeId = departmentDto.OfficeId;
                department.BuildingId = departmentDto.BuildingId;

                await _departmentRepository.UpdateDepartmentAsync(department);
            }
        }

    }
}

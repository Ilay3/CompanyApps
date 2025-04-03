using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Infrastructure.InterfacesRepository;
using CompanyApp.Core.Entities;
using CompanyApp.Core.Interfaces;


namespace CompanyApp.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public EmployeeService(IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesByDepartmentIdAsync(int departmentId)
        {
            var employees = await _employeeRepository.GetEmployeesByDepartmentIdAsync(departmentId);

            return _mapper.Map<IEnumerable<EmployeeDto>>(employees);
        }

        public async Task<EmployeeDto> GetEmployeeByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            return _mapper.Map<EmployeeDto>(employee);
        }

        public async Task<EmployeeDto> AddEmployeeAsync(EmployeeDto employeeDto)
        {
            var employee = _mapper.Map<Employee>(employeeDto);
            employee.DepartmentId = employeeDto.DepartmentId;
            employee = await _employeeRepository.AddEmployeeAsync(employee);
            return _mapper.Map<EmployeeDto>(employee);
        }





        public async Task DeleteEmployeeAsync(int id)
        {
            await _employeeRepository.DeleteEmployeeAsync(id);
        }



    }

}

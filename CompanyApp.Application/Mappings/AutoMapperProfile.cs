using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Computer, ComputerDto>()
            .ForMember(dest => dest.Employee, opt => opt.MapFrom(src => src.Employee))
            .ForMember(dest => dest.Monitors, opt => opt.MapFrom(src => src.DisplayMonitors));

            // Маппинг из DTO в сущности
            CreateMap<ComputerDto, Computer>();
            CreateMap<EmployeeDto, Employee>();
            CreateMap<DepartmentDto, Department>();
            CreateMap<OfficeDto, Office>();

            // Маппинг из сущностей в DTO
            CreateMap<Computer, ComputerDto>();
            CreateMap<Employee, EmployeeDto>();
            CreateMap<Department, DepartmentDto>();
            CreateMap<Office, OfficeDto>();

            CreateMap<CreateDepartmentDto, Department>();

            CreateMap<DisplayMonitor, DisplayMonitorDto>();

            CreateMap<DisplayMonitorDto, DisplayMonitor>().ReverseMap();

            CreateMap<Equipment, CRUDEquipmentDto>().ReverseMap();

            CreateMap<Office, OfficeDto>().ReverseMap();

            CreateMap<Building, BuildingDto>().ReverseMap();

            CreateMap<Department, DepartmentDto>().ReverseMap();

            CreateMap<Employee, EmployeeDto>().ReverseMap();

            CreateMap<Computer, ComputerDto>().ReverseMap();

            CreateMap<DisplayMonitor, DisplayMonitorDto>().ReverseMap();

            CreateMap<Equipment, EquipmentDto>().ReverseMap();
        }
    }
}

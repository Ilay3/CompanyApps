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
            // Существующие маппинги
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

            // Новые маппинги для программного обеспечения
            CreateMap<SoftwareLicense, SoftwareLicenseDto>()
                .ForMember(dest => dest.Installations, opt => opt.MapFrom(src => src.ComputerSoftware));
            CreateMap<SoftwareLicenseDto, SoftwareLicense>();

            CreateMap<ComputerSoftware, ComputerSoftwareDto>()
                .ForMember(dest => dest.Computer, opt => opt.MapFrom(src => src.Computer))
                .ForMember(dest => dest.SoftwareLicense, opt => opt.MapFrom(src => src.SoftwareLicense));
            CreateMap<ComputerSoftwareDto, ComputerSoftware>();

            // Новые маппинги для записей технического обслуживания
            CreateMap<MaintenanceRecord, MaintenanceRecordDto>()
                .ForMember(dest => dest.Computer, opt => opt.MapFrom(src => src.Computer))
                .ForMember(dest => dest.Equipment, opt => opt.MapFrom(src => src.Equipment));
            CreateMap<MaintenanceRecordDto, MaintenanceRecord>();
            CreateMap<CreateMaintenanceRecordDto, MaintenanceRecord>();
        }
    }
}
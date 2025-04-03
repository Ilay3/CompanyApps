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
    public class MonitorService : IMonitorService
    {
        private readonly IDisplayMonitorRepository _monitorRepository;
        private readonly IMapper _mapper;

        public MonitorService(IDisplayMonitorRepository monitorRepository, IMapper mapper)
        {
            _monitorRepository = monitorRepository;
            _mapper = mapper;
        }

        public IEnumerable<DisplayMonitorDto> GetMonitorsByComputerId(int computerId)
        {
            var monitors = _monitorRepository.GetByComputerId(computerId);
            return _mapper.Map<IEnumerable<DisplayMonitorDto>>(monitors);
        }

        public DisplayMonitorDto GetMonitorById(int id)
        {
            var monitor = _monitorRepository.GetById(id);
            return _mapper.Map<DisplayMonitorDto>(monitor);
        }

        public void AddMonitor(DisplayMonitorDto monitorDto)
        {
            var monitor = _mapper.Map<DisplayMonitor>(monitorDto);
            _monitorRepository.Add(monitor);
        }
    }

}

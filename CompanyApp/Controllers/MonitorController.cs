using CompanyApp.Application.DTOs;
using CompanyApp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CompanyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitorController : ControllerBase
    {
        private readonly IMonitorService _monitorService;

        public MonitorController(IMonitorService monitorService)
        {
            _monitorService = monitorService;
        }

        // Получить все мониторы по компьютеру
        [HttpGet("computer/{computerId}")]
        public IActionResult GetMonitorsByComputerId(int computerId)
        {
            var monitors = _monitorService.GetMonitorsByComputerId(computerId);
            return Ok(monitors);
        }

        
    }

}

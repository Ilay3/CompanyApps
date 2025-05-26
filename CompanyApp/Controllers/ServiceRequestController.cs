using CompanyApp.Application.DTOs;
using CompanyApp.Application.InterfacesService;
using CompanyApp.Core.Entities;
using CompanyApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace CompanyApp.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ServiceRequestController : Controller
    {
        private readonly IServiceRequestService _serviceRequestService;
        private readonly IComputerService _computerService;
        private readonly IEquipmentService _equipmentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceRequestController(
            IServiceRequestService serviceRequestService,
            IComputerService computerService,
            IEquipmentService equipmentService,
            UserManager<ApplicationUser> userManager)
        {
            _serviceRequestService = serviceRequestService;
            _computerService = computerService;
            _equipmentService = equipmentService;
            _userManager = userManager;
        }

        // GET: ServiceRequest или ServiceRequest/Index
        [HttpGet]
        [HttpGet("Index")]
        [Route("~/ServiceRequest")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "SysAdmin");

            IEnumerable<ServiceRequestDto> requests;

            if (isAdmin)
            {
                requests = await _serviceRequestService.GetAllServiceRequestsAsync();
            }
            else
            {
                requests = await _serviceRequestService.GetMyServiceRequestsAsync(user.Id);
            }

            return View(requests);
        }

        // GET: ServiceRequest/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var request = await _serviceRequestService.GetServiceRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "SysAdmin");

            // Проверка доступа
            if (!isAdmin && request.CreatedByUserId != currentUser.Id && request.AssignedToUserId != currentUser.Id)
            {
                return Forbid();
            }

            ViewBag.CurrentUserId = currentUser.Id;
            ViewBag.IsAdmin = isAdmin;

            return View(request);
        }

        // GET: ServiceRequest/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int? computerId = null, int? equipmentId = null)
        {
            var createDto = new CreateServiceRequestDto
            {
                ComputerId = computerId,
                EquipmentId = equipmentId,
                Priority = "Medium",
                RequestType = computerId.HasValue ? "Computer" : equipmentId.HasValue ? "Equipment" : "Other"
            };

            await PrepareCreateViewBag();

            return View(createDto);
        }

        // POST: ServiceRequest/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceRequestDto dto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var request = await _serviceRequestService.CreateServiceRequestAsync(dto, user.Id);

                TempData["Success"] = "Заявка успешно создана!";
                return RedirectToAction(nameof(Details), new { id = request.Id });
            }

            await PrepareCreateViewBag();
            return View(dto);
        }

        // GET: ServiceRequest/UpdateStatus/5
        [HttpGet("UpdateStatus/{id}")]
        [Authorize(Roles = "SysAdmin,Manager")]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var request = await _serviceRequestService.GetServiceRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateServiceRequestStatusDto
            {
                ServiceRequestId = id,
                NewStatus = request.Status
            };

            ViewBag.Request = request;
            ViewBag.Statuses = GetStatusSelectList(request.Status);

            return View(updateDto);
        }

        // POST: ServiceRequest/UpdateStatus
        [HttpPost("UpdateStatus")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SysAdmin,Manager")]
        public async Task<IActionResult> UpdateStatus(UpdateServiceRequestStatusDto dto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                await _serviceRequestService.UpdateServiceRequestStatusAsync(dto, user.Id);

                TempData["Success"] = "Статус заявки обновлен!";
                return RedirectToAction(nameof(Details), new { id = dto.ServiceRequestId });
            }

            var request = await _serviceRequestService.GetServiceRequestByIdAsync(dto.ServiceRequestId);
            ViewBag.Request = request;
            ViewBag.Statuses = GetStatusSelectList(request.Status);

            return View(dto);
        }

        // POST: ServiceRequest/AddComment
        [HttpPost("AddComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(AddCommentDto dto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                await _serviceRequestService.AddCommentAsync(dto, user.Id);

                TempData["Success"] = "Комментарий добавлен!";
            }

            return RedirectToAction(nameof(Details), new { id = dto.ServiceRequestId });
        }

        

        private async Task PrepareCreateViewBag()
        {
            ViewBag.Priorities = new SelectList(new[]
            {
                new { Value = "Low", Text = "Низкий" },
                new { Value = "Medium", Text = "Средний" },
                new { Value = "High", Text = "Высокий" },
                new { Value = "Critical", Text = "Критический" }
            }, "Value", "Text");

            ViewBag.RequestTypes = new SelectList(new[]
            {
                new { Value = "Computer", Text = "Компьютер" },
                new { Value = "Equipment", Text = "Оборудование" },
                new { Value = "Software", Text = "Программное обеспечение" },
                new { Value = "Other", Text = "Другое" }
            }, "Value", "Text");
        }

        private SelectList GetStatusSelectList(string currentStatus)
        {
            var statuses = new[]
            {
                new { Value = "New", Text = "Новая" },
                new { Value = "InProgress", Text = "В работе" },
                new { Value = "Resolved", Text = "Решена" },
                new { Value = "Closed", Text = "Закрыта" }
            };

            return new SelectList(statuses, "Value", "Text", currentStatus);
        }
    }
}
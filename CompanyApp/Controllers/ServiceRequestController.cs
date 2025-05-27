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
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

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
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при загрузке заявок: {ex.Message}";
                return View(new List<ServiceRequestDto>());
            }
        }

        // GET: ServiceRequest/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var request = await _serviceRequestService.GetServiceRequestByIdAsync(id);
                if (request == null)
                {
                    TempData["Error"] = "Заявка не найдена";
                    return RedirectToAction(nameof(Index));
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var isAdmin = await _userManager.IsInRoleAsync(currentUser, "SysAdmin");

                // Проверка доступа
                if (!isAdmin && request.CreatedByUserId != currentUser.Id && request.AssignedToUserId != currentUser.Id)
                {
                    TempData["Error"] = "У вас нет доступа к этой заявке";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.CurrentUserId = currentUser.Id;
                ViewBag.IsAdmin = isAdmin;

                return View(request);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при загрузке заявки: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: ServiceRequest/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create(int? computerId = null, int? equipmentId = null)
        {
            try
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
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при подготовке формы: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ServiceRequest/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceRequestDto dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    var request = await _serviceRequestService.CreateServiceRequestAsync(dto, user.Id);

                    TempData["Success"] = "Заявка успешно создана!";
                    return RedirectToAction(nameof(Details), new { id = request.Id });
                }

                await PrepareCreateViewBag();
                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при создании заявки: {ex.Message}";
                await PrepareCreateViewBag();
                return View(dto);
            }
        }

        // GET: ServiceRequest/UpdateStatus/5
        [HttpGet("UpdateStatus/{id}")]
        [Authorize(Roles = "SysAdmin,Manager")]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            try
            {
                // Проверяем, что ID валидный
                if (id <= 0)
                {
                    TempData["Error"] = "Неверный идентификатор заявки";
                    return RedirectToAction(nameof(Index));
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Получаем заявку
                var request = await _serviceRequestService.GetServiceRequestByIdAsync(id);
                if (request == null)
                {
                    TempData["Error"] = $"Заявка с ID {id} не найдена";
                    return RedirectToAction(nameof(Index));
                }

                // Проверяем, можно ли изменять статус этой заявки
                if (request.Status == "Closed")
                {
                    TempData["Warning"] = "Нельзя изменить статус закрытой заявки";
                    return RedirectToAction(nameof(Details), new { id });
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
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при загрузке формы изменения статуса: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ServiceRequest/UpdateStatus
        [HttpPost("UpdateStatus")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SysAdmin")]
        public async Task<IActionResult> UpdateStatus(UpdateServiceRequestStatusDto dto)
        {
            try
            {
                if (dto == null || dto.ServiceRequestId <= 0)
                {
                    TempData["Error"] = "Неверные данные запроса";
                    return RedirectToAction(nameof(Index));
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (ModelState.IsValid)
                {
                    // Проверяем существование заявки перед обновлением
                    var existingRequest = await _serviceRequestService.GetServiceRequestByIdAsync(dto.ServiceRequestId);
                    if (existingRequest == null)
                    {
                        TempData["Error"] = "Заявка не найдена";
                        return RedirectToAction(nameof(Index));
                    }

                    // Проверяем, можно ли изменить статус
                    if (existingRequest.Status == "Closed" && dto.NewStatus != "Closed")
                    {
                        TempData["Error"] = "Нельзя изменить статус закрытой заявки";
                        return RedirectToAction(nameof(Details), new { id = dto.ServiceRequestId });
                    }

                    // Валидация статуса
                    var validStatuses = new[] { "New", "InProgress", "Resolved", "Closed" };
                    if (!validStatuses.Contains(dto.NewStatus))
                    {
                        TempData["Error"] = "Неверный статус заявки";
                        return RedirectToAction(nameof(Details), new { id = dto.ServiceRequestId });
                    }

                    await _serviceRequestService.UpdateServiceRequestStatusAsync(dto, currentUser.Id);

                    TempData["Success"] = "Статус заявки успешно обновлен!";
                    return RedirectToAction(nameof(Details), new { id = dto.ServiceRequestId });
                }

                // Если ModelState не валидна, возвращаем форму с ошибками
                var request = await _serviceRequestService.GetServiceRequestByIdAsync(dto.ServiceRequestId);
                if (request == null)
                {
                    TempData["Error"] = "Заявка не найдена";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Request = request;
                ViewBag.Statuses = GetStatusSelectList(dto.NewStatus);

                return View(dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при обновлении статуса: {ex.Message}";

                // Пытаемся вернуться к детальной странице заявки
                if (dto?.ServiceRequestId > 0)
                {
                    return RedirectToAction(nameof(Details), new { id = dto.ServiceRequestId });
                }

                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ServiceRequest/AddComment
        [HttpPost("AddComment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(AddCommentDto dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    await _serviceRequestService.AddCommentAsync(dto, user.Id);

                    TempData["Success"] = "Комментарий добавлен!";
                }
                else
                {
                    TempData["Error"] = "Ошибка при добавлении комментария. Проверьте введенные данные.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при добавлении комментария: {ex.Message}";
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

        private string TranslateStatus(string status)
        {
            return status switch
            {
                "New" => "Новая",
                "InProgress" => "В работе",
                "Resolved" => "Решена",
                "Closed" => "Закрыта",
                _ => status
            };
        }

    }
}
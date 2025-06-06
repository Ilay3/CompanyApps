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

        [HttpGet("Details/{id:int}")]
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
        public async Task<IActionResult> CreatePost(CreateServiceRequestDto dto)
        {
            try
            {
                if (dto == null)
                {
                    TempData["Error"] = "Некорректные данные формы";
                    return RedirectToAction(nameof(Index));
                }

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
                return View("Create", dto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при создании заявки: {ex.Message}";

                if (dto == null)
                {
                    dto = new CreateServiceRequestDto();
                }

                await PrepareCreateViewBag();
                return View("Create", dto);
            }
        }

        // GET: ServiceRequest/Delete/5
        [HttpGet("Delete/{id:int}")]
        [Authorize(Roles = "SysAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var request = await _serviceRequestService.GetServiceRequestByIdAsync(id);
                if (request == null)
                {
                    TempData["Error"] = "Заявка не найдена";
                    return RedirectToAction(nameof(Index));
                }

                if (request.Status == "InProgress")
                {
                    TempData["Error"] = "Нельзя удалить заявку, которая находится в работе";
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(request);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при загрузке заявки: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ServiceRequest/Delete/5
        [HttpPost("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SysAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var request = await _serviceRequestService.GetServiceRequestByIdAsync(id);
                if (request == null)
                {
                    TempData["Error"] = "Заявка не найдена";
                    return RedirectToAction(nameof(Index));
                }

                if (request.Status == "InProgress")
                {
                    TempData["Error"] = "Нельзя удалить заявку, которая находится в работе";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _serviceRequestService.DeleteServiceRequestAsync(id);

                TempData["Success"] = "Заявка успешно удалена";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при удалении заявки: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
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



        // GET: ServiceRequest/UpdateStatus/5
        [HttpGet("UpdateStatus/{id:int}")]
        [Authorize(Roles = "SysAdmin,Manager")]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            Console.WriteLine($"[CONTROLLER] GET UpdateStatus вызван для ID: {id}");

            try
            {
                if (id <= 0)
                {
                    Console.WriteLine($"[CONTROLLER] Неверный ID: {id}");
                    TempData["Error"] = "Неверный идентификатор заявки";
                    return RedirectToAction(nameof(Index));
                }

                var request = await _serviceRequestService.GetServiceRequestByIdAsync(id);
                if (request == null)
                {
                    Console.WriteLine($"[CONTROLLER] Заявка с ID {id} не найдена");
                    TempData["Error"] = $"Заявка с ID {id} не найдена";
                    return RedirectToAction(nameof(Index));
                }

                Console.WriteLine($"[CONTROLLER] Заявка найдена. Статус: {request.Status}");

                if (request.Status == "Closed")
                {
                    Console.WriteLine($"[CONTROLLER] Заявка уже закрыта");
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

                Console.WriteLine($"[CONTROLLER] Возвращаем представление UpdateStatus");
                return View(updateDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] Ошибка в GET UpdateStatus: {ex.Message}");
                TempData["Error"] = $"Ошибка при загрузке формы: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ServiceRequest/UpdateStatus
        [HttpPost("UpdateStatus")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SysAdmin,Manager")]
        public async Task<IActionResult> UpdateStatusPost(UpdateServiceRequestStatusDto dto)
        {
            Console.WriteLine($"[CONTROLLER] POST UpdateStatus вызван");
            Console.WriteLine($"[CONTROLLER] ServiceRequestId: {dto?.ServiceRequestId}");
            Console.WriteLine($"[CONTROLLER] NewStatus: {dto?.NewStatus}");
            Console.WriteLine($"[CONTROLLER] Reason: {dto?.Reason}");
            Console.WriteLine($"[CONTROLLER] Resolution: {dto?.Resolution}");

            try
            {
                if (dto?.ServiceRequestId <= 0)
                {
                    Console.WriteLine($"[CONTROLLER] Неверные данные DTO");
                    TempData["Error"] = "Неверные данные запроса";
                    return RedirectToAction(nameof(Index));
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    Console.WriteLine($"[CONTROLLER] Пользователь не найден");
                    return RedirectToAction("Login", "Account");
                }

                Console.WriteLine($"[CONTROLLER] Текущий пользователь: {currentUser.Email}, ID: {currentUser.Id}");

                // Проверяем существование заявки
                var existingRequest = await _serviceRequestService.GetServiceRequestByIdAsync(dto.ServiceRequestId);
                if (existingRequest == null)
                {
                    Console.WriteLine($"[CONTROLLER] Заявка для обновления не найдена");
                    TempData["Error"] = "Заявка не найдена";
                    return RedirectToAction(nameof(Index));
                }

                Console.WriteLine($"[CONTROLLER] Существующая заявка найдена. Статус: {existingRequest.Status}");

                // Проверяем, можно ли изменить статус
                if (existingRequest.Status == "Closed")
                {
                    Console.WriteLine($"[CONTROLLER] Попытка изменить статус закрытой заявки");
                    TempData["Error"] = "Нельзя изменить статус закрытой заявки";
                    return RedirectToAction(nameof(Details), new { id = dto.ServiceRequestId });
                }

                // Валидация статуса
                var validStatuses = new[] { "New", "InProgress", "Resolved", "Closed" };
                if (string.IsNullOrEmpty(dto.NewStatus) || !validStatuses.Contains(dto.NewStatus))
                {
                    Console.WriteLine($"[CONTROLLER] Неверный статус: {dto.NewStatus}");
                    var request = await _serviceRequestService.GetServiceRequestByIdAsync(dto.ServiceRequestId);
                    ViewBag.Request = request;
                    ViewBag.Statuses = GetStatusSelectList(dto.NewStatus);
                    ModelState.AddModelError("NewStatus", "Неверный статус заявки");
                    return View("UpdateStatus", dto);
                }

                // Проверяем, требуется ли описание решения
                if ((dto.NewStatus == "Resolved" || dto.NewStatus == "Closed") && string.IsNullOrWhiteSpace(dto.Resolution))
                {
                    Console.WriteLine($"[CONTROLLER] Отсутствует описание решения для статуса {dto.NewStatus}");
                    var request = await _serviceRequestService.GetServiceRequestByIdAsync(dto.ServiceRequestId);
                    ViewBag.Request = request;
                    ViewBag.Statuses = GetStatusSelectList(dto.NewStatus);
                    ModelState.AddModelError("Resolution", "Описание решения обязательно при закрытии заявки");
                    return View("UpdateStatus", dto);
                }

                Console.WriteLine($"[CONTROLLER] Вызываем сервис UpdateServiceRequestStatusAsync");

                // Обновляем статус
                await _serviceRequestService.UpdateServiceRequestStatusAsync(dto, currentUser.Id);

                Console.WriteLine($"[CONTROLLER] Сервис выполнен успешно");
                TempData["Success"] = "Статус заявки успешно обновлен!";

                return RedirectToAction(nameof(Details), new { id = dto.ServiceRequestId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONTROLLER] Ошибка в POST UpdateStatus: {ex.Message}");
                Console.WriteLine($"[CONTROLLER] StackTrace: {ex.StackTrace}");

                TempData["Error"] = $"Ошибка при обновлении статуса: {ex.Message}";

                if (dto?.ServiceRequestId > 0)
                {
                    return RedirectToAction(nameof(Details), new { id = dto.ServiceRequestId });
                }

                return RedirectToAction(nameof(Index));
            }
        }


        private async Task PrepareCreateViewBag()
        {
            try
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
            catch (Exception ex)
            {
                // Если не удается подготовить ViewBag, создаем пустые списки
                ViewBag.Priorities = new SelectList(new List<object>(), "Value", "Text");
                ViewBag.RequestTypes = new SelectList(new List<object>(), "Value", "Text");

                Console.WriteLine($"Ошибка при подготовке ViewBag: {ex.Message}");
            }
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
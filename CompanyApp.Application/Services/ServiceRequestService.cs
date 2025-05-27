using AutoMapper;
using CompanyApp.Application.DTOs;
using CompanyApp.Application.InterfacesService;
using CompanyApp.Core.Entities;
using CompanyApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyApp.Application.Services
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly OfficeDbContext _context;
        private readonly IMapper _mapper;

        public ServiceRequestService(OfficeDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetAllServiceRequestsAsync()
        {
            try
            {
                var requests = await _context.ServiceRequests
                    .AsSplitQuery() // Используем разделение запросов
                    .Include(sr => sr.CreatedByUser)
                    .Include(sr => sr.AssignedToUser)
                    .Include(sr => sr.Computer)
                    .Include(sr => sr.Equipment)
                    .OrderByDescending(sr => sr.CreatedDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении списка заявок: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetMyServiceRequestsAsync(string userId)
        {
            try
            {
                var requests = await _context.ServiceRequests
                    .AsSplitQuery()
                    .Include(sr => sr.CreatedByUser)
                    .Include(sr => sr.AssignedToUser)
                    .Include(sr => sr.Computer)
                    .Include(sr => sr.Equipment)
                    .Where(sr => sr.CreatedByUserId == userId)
                    .OrderByDescending(sr => sr.CreatedDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении заявок пользователя: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetAssignedServiceRequestsAsync(string userId)
        {
            try
            {
                var requests = await _context.ServiceRequests
                    .AsSplitQuery()
                    .Include(sr => sr.CreatedByUser)
                    .Include(sr => sr.AssignedToUser)
                    .Include(sr => sr.Computer)
                    .Include(sr => sr.Equipment)
                    .Where(sr => sr.AssignedToUserId == userId)
                    .OrderByDescending(sr => sr.CreatedDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении назначенных заявок: {ex.Message}", ex);
            }
        }

        public async Task<ServiceRequestDto> GetServiceRequestByIdAsync(int id)
        {
            try
            {
                // Загружаем основную заявку
                var request = await _context.ServiceRequests
                    .Include(sr => sr.CreatedByUser)
                    .Include(sr => sr.AssignedToUser)
                    .Include(sr => sr.Computer)
                        .ThenInclude(c => c.Employee)
                    .Include(sr => sr.Equipment)
                        .ThenInclude(e => e.Department)
                    .FirstOrDefaultAsync(sr => sr.Id == id);

                if (request == null)
                {
                    return null;
                }

                // Загружаем историю статусов отдельно
                var statusHistories = await _context.ServiceRequestStatusHistories
                    .Include(sh => sh.ChangedByUser)
                    .Where(sh => sh.ServiceRequestId == id)
                    .OrderByDescending(sh => sh.ChangedDate)
                    .ToListAsync();

                // Загружаем комментарии отдельно
                var comments = await _context.ServiceRequestComments
                    .Include(c => c.User)
                    .Where(c => c.ServiceRequestId == id)
                    .OrderByDescending(c => c.CreatedDate)
                    .ToListAsync();

                // Присваиваем коллекции
                request.StatusHistories = statusHistories;
                request.Comments = comments;

                return _mapper.Map<ServiceRequestDto>(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении заявки с ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<ServiceRequestDto> CreateServiceRequestAsync(CreateServiceRequestDto dto, string userId)
        {
            try
            {
                var request = new ServiceRequest
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    CreatedDate = DateTime.Now,
                    Status = "New",
                    Priority = dto.Priority,
                    RequestType = dto.RequestType,
                    ComputerId = dto.ComputerId,
                    EquipmentId = dto.EquipmentId,
                    CreatedByUserId = userId
                };

                _context.ServiceRequests.Add(request);
                await _context.SaveChangesAsync();

                // Создаем запись в истории статусов
                var statusHistory = new ServiceRequestStatusHistory
                {
                    ServiceRequestId = request.Id,
                    OldStatus = "",
                    NewStatus = "New",
                    ChangedDate = DateTime.Now,
                    ChangedByUserId = userId,
                    Reason = "Создана новая заявка"
                };

                _context.ServiceRequestStatusHistories.Add(statusHistory);
                await _context.SaveChangesAsync();

                return await GetServiceRequestByIdAsync(request.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании заявки: {ex.Message}", ex);
            }
        }

        public async Task UpdateServiceRequestStatusAsync(UpdateServiceRequestStatusDto dto, string userId)
        {
            try
            {
                var request = await _context.ServiceRequests
                    .FirstOrDefaultAsync(sr => sr.Id == dto.ServiceRequestId);

                if (request == null)
                    throw new Exception("Заявка не найдена");

                var oldStatus = request.Status;

                // Обновляем статус заявки
                request.Status = dto.NewStatus;

                if (dto.NewStatus == "Resolved" || dto.NewStatus == "Closed")
                {
                    request.ResolvedDate = DateTime.Now;
                }

                // Создаем запись в истории
                var statusHistory = new ServiceRequestStatusHistory
                {
                    ServiceRequestId = request.Id,
                    OldStatus = oldStatus,
                    NewStatus = dto.NewStatus,
                    Reason = dto.Reason,
                    Resolution = dto.Resolution,
                    ChangedDate = DateTime.Now,
                    ChangedByUserId = userId
                };

                _context.ServiceRequestStatusHistories.Add(statusHistory);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обновлении статуса заявки: {ex.Message}", ex);
            }
        }

        public async Task AssignServiceRequestAsync(int requestId, string assignToUserId, string currentUserId)
        {
            try
            {
                var request = await _context.ServiceRequests
                    .FirstOrDefaultAsync(sr => sr.Id == requestId);

                if (request == null)
                    throw new Exception("Заявка не найдена");

                var oldStatus = request.Status;
                request.AssignedToUserId = assignToUserId;

                if (request.Status == "New")
                {
                    request.Status = "InProgress";
                }

                var statusHistory = new ServiceRequestStatusHistory
                {
                    ServiceRequestId = request.Id,
                    OldStatus = oldStatus,
                    NewStatus = request.Status,
                    ChangedDate = DateTime.Now,
                    ChangedByUserId = currentUserId,
                    Reason = "Заявка назначена на исполнителя"
                };

                _context.ServiceRequestStatusHistories.Add(statusHistory);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при назначении заявки: {ex.Message}", ex);
            }
        }

        public async Task AddCommentAsync(AddCommentDto dto, string userId)
        {
            try
            {
                var comment = new ServiceRequestComment
                {
                    ServiceRequestId = dto.ServiceRequestId,
                    Comment = dto.Comment,
                    CreatedDate = DateTime.Now,
                    UserId = userId
                };

                _context.ServiceRequestComments.Add(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при добавлении комментария: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsByStatusAsync(string status)
        {
            try
            {
                var requests = await _context.ServiceRequests
                    .AsSplitQuery()
                    .Include(sr => sr.CreatedByUser)
                    .Include(sr => sr.AssignedToUser)
                    .Where(sr => sr.Status == status)
                    .OrderByDescending(sr => sr.CreatedDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении заявок по статусу: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsByComputerIdAsync(int computerId)
        {
            try
            {
                var requests = await _context.ServiceRequests
                    .AsSplitQuery()
                    .Include(sr => sr.CreatedByUser)
                    .Include(sr => sr.AssignedToUser)
                    .Where(sr => sr.ComputerId == computerId)
                    .OrderByDescending(sr => sr.CreatedDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении заявок по компьютеру: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsByEquipmentIdAsync(int equipmentId)
        {
            try
            {
                var requests = await _context.ServiceRequests
                    .AsSplitQuery()
                    .Include(sr => sr.CreatedByUser)
                    .Include(sr => sr.AssignedToUser)
                    .Where(sr => sr.EquipmentId == equipmentId)
                    .OrderByDescending(sr => sr.CreatedDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении заявок по оборудованию: {ex.Message}", ex);
            }
        }
    }
}
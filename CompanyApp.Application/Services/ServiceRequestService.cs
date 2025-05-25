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
            var requests = await _context.ServiceRequests
                .Include(sr => sr.CreatedByUser)
                .Include(sr => sr.AssignedToUser)
                .Include(sr => sr.Computer)
                .Include(sr => sr.Equipment)
                .Include(sr => sr.StatusHistories)
                    .ThenInclude(sh => sh.ChangedByUser)
                .Include(sr => sr.Comments)
                    .ThenInclude(c => c.User)
                .OrderByDescending(sr => sr.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetMyServiceRequestsAsync(string userId)
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.CreatedByUser)
                .Include(sr => sr.AssignedToUser)
                .Include(sr => sr.Computer)
                .Include(sr => sr.Equipment)
                .Where(sr => sr.CreatedByUserId == userId)
                .OrderByDescending(sr => sr.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetAssignedServiceRequestsAsync(string userId)
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.CreatedByUser)
                .Include(sr => sr.AssignedToUser)
                .Include(sr => sr.Computer)
                .Include(sr => sr.Equipment)
                .Where(sr => sr.AssignedToUserId == userId)
                .OrderByDescending(sr => sr.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
        }

        public async Task<ServiceRequestDto> GetServiceRequestByIdAsync(int id)
        {
            var request = await _context.ServiceRequests
                .Include(sr => sr.CreatedByUser)
                .Include(sr => sr.AssignedToUser)
                .Include(sr => sr.Computer)
                    .ThenInclude(c => c.Employee)
                .Include(sr => sr.Equipment)
                    .ThenInclude(e => e.Department)
                .Include(sr => sr.StatusHistories)
                    .ThenInclude(sh => sh.ChangedByUser)
                .Include(sr => sr.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(sr => sr.Id == id);

            return _mapper.Map<ServiceRequestDto>(request);
        }

        public async Task<ServiceRequestDto> CreateServiceRequestAsync(CreateServiceRequestDto dto, string userId)
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
                CreatedByUserId = userId,
                StatusHistories = new List<ServiceRequestStatusHistory>
                {
                    new ServiceRequestStatusHistory
                    {
                        OldStatus = "",
                        NewStatus = "New",
                        ChangedDate = DateTime.Now,
                        ChangedByUserId = userId,
                        Reason = "Создана новая заявка"
                    }
                }
            };

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();

            return await GetServiceRequestByIdAsync(request.Id);
        }

        public async Task UpdateServiceRequestStatusAsync(UpdateServiceRequestStatusDto dto, string userId)
        {
            var request = await _context.ServiceRequests
                .Include(sr => sr.StatusHistories)
                .FirstOrDefaultAsync(sr => sr.Id == dto.ServiceRequestId);

            if (request == null)
                throw new Exception("Заявка не найдена");

            var statusHistory = new ServiceRequestStatusHistory
            {
                ServiceRequestId = request.Id,
                OldStatus = request.Status,
                NewStatus = dto.NewStatus,
                Reason = dto.Reason,
                Resolution = dto.Resolution,
                ChangedDate = DateTime.Now,
                ChangedByUserId = userId
            };

            request.Status = dto.NewStatus;

            if (dto.NewStatus == "Resolved" || dto.NewStatus == "Closed")
            {
                request.ResolvedDate = DateTime.Now;
            }

            request.StatusHistories.Add(statusHistory);

            await _context.SaveChangesAsync();
        }

        public async Task AssignServiceRequestAsync(int requestId, string assignToUserId, string currentUserId)
        {
            var request = await _context.ServiceRequests
                .Include(sr => sr.StatusHistories)
                .FirstOrDefaultAsync(sr => sr.Id == requestId);

            if (request == null)
                throw new Exception("Заявка не найдена");

            var oldAssignedUserId = request.AssignedToUserId;
            request.AssignedToUserId = assignToUserId;

            if (request.Status == "New")
            {
                request.Status = "InProgress";
            }

            var statusHistory = new ServiceRequestStatusHistory
            {
                ServiceRequestId = request.Id,
                OldStatus = request.Status,
                NewStatus = request.Status,
                ChangedDate = DateTime.Now,
                ChangedByUserId = currentUserId,
                Reason = $"Заявка назначена на исполнителя"
            };

            request.StatusHistories.Add(statusHistory);

            await _context.SaveChangesAsync();
        }

        public async Task AddCommentAsync(AddCommentDto dto, string userId)
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

        public async Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsByStatusAsync(string status)
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.CreatedByUser)
                .Include(sr => sr.AssignedToUser)
                .Where(sr => sr.Status == status)
                .OrderByDescending(sr => sr.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsByComputerIdAsync(int computerId)
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.CreatedByUser)
                .Include(sr => sr.AssignedToUser)
                .Where(sr => sr.ComputerId == computerId)
                .OrderByDescending(sr => sr.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
        }

        public async Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsByEquipmentIdAsync(int equipmentId)
        {
            var requests = await _context.ServiceRequests
                .Include(sr => sr.CreatedByUser)
                .Include(sr => sr.AssignedToUser)
                .Where(sr => sr.EquipmentId == equipmentId)
                .OrderByDescending(sr => sr.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ServiceRequestDto>>(requests);
        }
    }
}
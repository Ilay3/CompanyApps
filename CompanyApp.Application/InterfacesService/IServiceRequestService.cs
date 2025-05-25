using CompanyApp.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompanyApp.Application.InterfacesService
{
    public interface IServiceRequestService
    {
        Task<IEnumerable<ServiceRequestDto>> GetAllServiceRequestsAsync();
        Task<IEnumerable<ServiceRequestDto>> GetMyServiceRequestsAsync(string userId);
        Task<IEnumerable<ServiceRequestDto>> GetAssignedServiceRequestsAsync(string userId);
        Task<ServiceRequestDto> GetServiceRequestByIdAsync(int id);
        Task<ServiceRequestDto> CreateServiceRequestAsync(CreateServiceRequestDto dto, string userId);
        Task UpdateServiceRequestStatusAsync(UpdateServiceRequestStatusDto dto, string userId);
        Task AssignServiceRequestAsync(int requestId, string assignToUserId, string currentUserId);
        Task AddCommentAsync(AddCommentDto dto, string userId);
        Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsByStatusAsync(string status);
        Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsByComputerIdAsync(int computerId);
        Task<IEnumerable<ServiceRequestDto>> GetServiceRequestsByEquipmentIdAsync(int equipmentId);
    }
}
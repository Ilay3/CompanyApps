using Microsoft.AspNetCore.Identity;

namespace CompanyApp.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Навигационные свойства для заявок
        public ICollection<ServiceRequest>? CreatedRequests { get; set; }
        public ICollection<ServiceRequest>? AssignedRequests { get; set; }
    }
}
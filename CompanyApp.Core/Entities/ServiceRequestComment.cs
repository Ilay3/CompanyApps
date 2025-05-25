namespace CompanyApp.Core.Entities
{
    public class ServiceRequestComment
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public ServiceRequest ServiceRequest { get; set; }

        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
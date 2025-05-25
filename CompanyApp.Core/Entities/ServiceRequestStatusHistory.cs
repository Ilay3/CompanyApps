namespace CompanyApp.Core.Entities
{
    public class ServiceRequestStatusHistory
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public ServiceRequest ServiceRequest { get; set; }

        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public string? Reason { get; set; }
        public string? Resolution { get; set; }

        public DateTime ChangedDate { get; set; }
        public string ChangedByUserId { get; set; }
        public ApplicationUser ChangedByUser { get; set; }
    }
}
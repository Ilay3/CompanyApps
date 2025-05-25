using CompanyApp.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CompanyApp.Infrastructure.Data
{
    public class OfficeDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Office> Offices { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<DisplayMonitor> DisplayMonitors { get; set; }
        public DbSet<SoftwareLicense> SoftwareLicenses { get; set; }
        public DbSet<ComputerSoftware> ComputerSoftware { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<ServiceRequestStatusHistory> ServiceRequestStatusHistories { get; set; }
        public DbSet<ServiceRequestComment> ServiceRequestComments { get; set; }

        public OfficeDbContext(DbContextOptions<OfficeDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Существующие отношения
            modelBuilder.Entity<Office>()
                .HasMany(o => o.Buildings)
                .WithOne(b => b.Office)
                .HasForeignKey(b => b.OfficeId);

            modelBuilder.Entity<Office>()
                .HasMany(o => o.Departments)
                .WithOne(d => d.Office)
                .HasForeignKey(d => d.OfficeId);

            modelBuilder.Entity<Building>()
                .HasMany(b => b.Departments)
                .WithOne(d => d.Building)
                .HasForeignKey(d => d.BuildingId);

            modelBuilder.Entity<Department>()
                .HasMany(d => d.Employees)
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.DepartmentId);

            modelBuilder.Entity<Department>()
                .HasMany(d => d.Equipments)
                .WithOne(eq => eq.Department)
                .HasForeignKey(eq => eq.DepartmentId);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Computers)
                .WithOne(c => c.Employee)
                .HasForeignKey(c => c.EmployeeId);

            modelBuilder.Entity<Computer>()
                .HasMany(c => c.DisplayMonitors)
                .WithOne(m => m.Computer)
                .HasForeignKey(m => m.ComputerId);

            // Отношение Many-to-Many между Computer и SoftwareLicense через ComputerSoftware
            modelBuilder.Entity<ComputerSoftware>()
                .HasOne(cs => cs.Computer)
                .WithMany()
                .HasForeignKey(cs => cs.ComputerId);

            modelBuilder.Entity<ComputerSoftware>()
                .HasOne(cs => cs.SoftwareLicense)
                .WithMany(s => s.ComputerSoftware)
                .HasForeignKey(cs => cs.SoftwareLicenseId);

            // Настройка отношений для MaintenanceRecord
            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(mr => mr.Computer)
                .WithMany()
                .HasForeignKey(mr => mr.ComputerId)
                .IsRequired(false);

            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(mr => mr.Equipment)
                .WithMany()
                .HasForeignKey(mr => mr.EquipmentId)
                .IsRequired(false);

            // Настройка отношений для ServiceRequest
            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.CreatedByUser)
                .WithMany(u => u.CreatedRequests)
                .HasForeignKey(sr => sr.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.AssignedToUser)
                .WithMany(u => u.AssignedRequests)
                .HasForeignKey(sr => sr.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Computer)
                .WithMany()
                .HasForeignKey(sr => sr.ComputerId)
                .IsRequired(false);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Equipment)
                .WithMany()
                .HasForeignKey(sr => sr.EquipmentId)
                .IsRequired(false);

            // Настройка отношений для истории статусов
            modelBuilder.Entity<ServiceRequestStatusHistory>()
                .HasOne(h => h.ServiceRequest)
                .WithMany(sr => sr.StatusHistories)
                .HasForeignKey(h => h.ServiceRequestId);

            modelBuilder.Entity<ServiceRequestStatusHistory>()
                .HasOne(h => h.ChangedByUser)
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка отношений для комментариев
            modelBuilder.Entity<ServiceRequestComment>()
                .HasOne(c => c.ServiceRequest)
                .WithMany(sr => sr.Comments)
                .HasForeignKey(c => c.ServiceRequestId);

            modelBuilder.Entity<ServiceRequestComment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Связь ApplicationUser с Employee
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Employee)
                .WithMany()
                .HasForeignKey(u => u.EmployeeId)
                .IsRequired(false);
        }
    }
}
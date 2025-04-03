using CompanyApp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


namespace CompanyApp.Infrastructure.Data
{
    public class OfficeDbContext : DbContext
    {
        public DbSet<Office> Offices { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<DisplayMonitor> DisplayMonitors { get; set; }

        // Новые DbSets для программного обеспечения и ТО
        public DbSet<SoftwareLicense> SoftwareLicenses { get; set; }
        public DbSet<ComputerSoftware> ComputerSoftware { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }

        public OfficeDbContext(DbContextOptions<OfficeDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            // Новые отношения

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
                .IsRequired(false); // Допускается отсутствие связи с компьютером

            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(mr => mr.Equipment)
                .WithMany()
                .HasForeignKey(mr => mr.EquipmentId)
                .IsRequired(false); // Допускается отсутствие связи с оборудованием
        }
    }
}
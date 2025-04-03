using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Core.Entities
{
    public class SoftwareLicense
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string LicenseKey { get; set; }
        public DateTime PurchaseDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string LicenseType { get; set; }
        public int Seats { get; set; }
        public int SeatsUsed { get; set; }
        public decimal Cost { get; set; }
        public string? Notes { get; set; }

        // Связи с компьютерами, на которых установлено ПО
        public ICollection<ComputerSoftware>? ComputerSoftware { get; set; }
    }

    // Связующая сущность для Many-to-Many между компьютерами и ПО
    public class ComputerSoftware
    {
        public int Id { get; set; }
        public int ComputerId { get; set; }
        public Computer Computer { get; set; }
        public int SoftwareLicenseId { get; set; }
        public SoftwareLicense SoftwareLicense { get; set; }
        public DateTime InstallationDate { get; set; }
        public string? Notes { get; set; }
    }
}
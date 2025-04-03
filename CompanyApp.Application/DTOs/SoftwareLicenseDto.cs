using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApp.Application.DTOs
{
    public class SoftwareLicenseDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название программного обеспечения обязательно")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Версия программного обеспечения обязательна")]
        public string Version { get; set; }

        [Required(ErrorMessage = "Лицензионный ключ обязателен")]
        public string LicenseKey { get; set; }

        [Required(ErrorMessage = "Дата покупки обязательна")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PurchaseDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ExpirationDate { get; set; }

        [Required(ErrorMessage = "Тип лицензии обязателен")]
        public string LicenseType { get; set; }

        [Required(ErrorMessage = "Количество лицензий обязательно")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество лицензий должно быть больше 0")]
        public int Seats { get; set; }

        public int SeatsUsed { get; set; }

        [Required(ErrorMessage = "Стоимость обязательна")]
        [Range(0, double.MaxValue, ErrorMessage = "Стоимость должна быть положительным числом")]
        public decimal Cost { get; set; }

        public string? Notes { get; set; }

        public List<ComputerSoftwareDto> Installations { get; set; } = new List<ComputerSoftwareDto>();
    }

    public class ComputerSoftwareDto
    {
        public int Id { get; set; }
        public int ComputerId { get; set; }
        public ComputerDto? Computer { get; set; }
        public int SoftwareLicenseId { get; set; }
        public SoftwareLicenseDto? SoftwareLicense { get; set; }

        [Required(ErrorMessage = "Дата установки обязательна")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime InstallationDate { get; set; }

        public string? Notes { get; set; }
    }

    public class AssignSoftwareDto
    {
        public int SoftwareLicenseId { get; set; }
        public List<int> ComputerIds { get; set; } = new List<int>();
        public DateTime InstallationDate { get; set; } = DateTime.Now;
        public string? Notes { get; set; }
    }
}
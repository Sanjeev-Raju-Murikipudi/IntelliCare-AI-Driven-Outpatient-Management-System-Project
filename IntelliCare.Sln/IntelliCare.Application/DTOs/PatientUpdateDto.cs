using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.DTOs
{
    public class PatientUpdateDto
    {
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; }

        [Required]
        public DateTime DOB { get; set; }

        [Required]
        public string InsuranceDetails { get; set; }

        [Required]
        public string MedicalHistory { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }
    }
}

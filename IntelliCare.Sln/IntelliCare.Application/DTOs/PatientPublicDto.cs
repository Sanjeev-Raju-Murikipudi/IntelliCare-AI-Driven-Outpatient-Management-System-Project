using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.DTOs
{
    public class PatientPublicDto
    {
        public int PatientId { get; set; }
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? ContactEmail { get; set; }
        public DateTime DOB { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = null!;
        public string BloodGroup { get; set; } = null!;
        public string InsuranceDetails { get; set; } = null!;
        public string MedicalHistory { get; set; } = null!;
        public string Address { get; set; } = null!;
    }
}

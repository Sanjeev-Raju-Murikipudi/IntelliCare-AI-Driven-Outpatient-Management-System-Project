using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.DTOs
{
    public class PatientSummaryDto
    {
        public int PatientId { get; set; }
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Gender { get; set; } = null!;
        public string BloodGroup { get; set; } = null!;
        public string MedicalHistory { get; set; } = null!;
        public int Age { get; set; } 
    }
}

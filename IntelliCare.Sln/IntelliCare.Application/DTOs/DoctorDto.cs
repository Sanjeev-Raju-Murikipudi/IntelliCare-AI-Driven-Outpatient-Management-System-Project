using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.DTOs
{
    public class DoctorDto
    {    
        public int DoctorId { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Specialization { get; set; }
        public string Education { get; set; }
        public string Address { get; set; }
        public int ExperienceYears { get; set; }
        public string PhotoUrl { get; set; }
    }
}


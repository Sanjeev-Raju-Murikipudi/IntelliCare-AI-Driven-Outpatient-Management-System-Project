using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.DTOs
{
    public class CreateSlotDto
    {
        public int DoctorID { get; set; }
        public DateTime Date { get; set; } // e.g. 2025-10-10
        public string StartTime { get; set; } // e.g. "09:00"
        public string EndTime { get; set; }   // e.g. "12:00"
        public int IntervalMinutes { get; set; } // e.g. 30
        public decimal Fee { get; set; } // e.g. 500
    }
}

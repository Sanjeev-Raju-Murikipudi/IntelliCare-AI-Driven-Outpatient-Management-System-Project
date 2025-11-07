using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.DTOs
{
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string Status { get; set; }
        public string PatientName { get; set; }
        public string Reason { get; set; }
        public string DoctorName { get; set; }
        public decimal AmountPaid { get; set; }
    }
}

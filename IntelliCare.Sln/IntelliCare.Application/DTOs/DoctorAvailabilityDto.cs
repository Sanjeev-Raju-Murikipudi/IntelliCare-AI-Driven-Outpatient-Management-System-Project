using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.DTOs
{
    public class DoctorAvailabilityDto
    {
        public int SlotID { get; set; }
        public int DoctorID { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime Date_Time { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal AppointmentFee { get; set; }
    }
}

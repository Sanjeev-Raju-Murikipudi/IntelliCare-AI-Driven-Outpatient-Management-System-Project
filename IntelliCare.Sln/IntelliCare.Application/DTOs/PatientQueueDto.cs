using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.DTOs
{
    public class PatientQueueDto
    {
        public int DoctorID { get; set; }
        public int PatientID { get; set; }
        public DateTime Date_Time { get; set; }
        public int SlotDurationMinutes { get; set; }
        public int QueuePosition { get; set; }
    }
}

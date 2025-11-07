using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.DTOs
{
    public class BookAppointmentResult
    {
        public string Message { get; set; }
        public decimal Fee { get; set; }
        public bool PaymentRequired { get; set; }
        public int AppointmentID { get; set; }
    }
}

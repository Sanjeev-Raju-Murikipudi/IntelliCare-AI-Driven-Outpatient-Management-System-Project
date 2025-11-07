using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliCare.Application.DTOs
{
    public class CancelAppointmentDto
    {
        [Required]
        public int AppointmentID { get; set; }
    }

}

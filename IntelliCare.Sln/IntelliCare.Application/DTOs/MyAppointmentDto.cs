using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

namespace IntelliCare.Application.DTOs

{

    public class MyAppointmentDto

    {

        public int AppointmentID { get; set; }

        public int DoctorID { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public string? DoctorPhotoUrl { get; set; }

        public string? DoctorSpecialization { get; set; }

        public string? DoctorAddress { get; set; }

        public string Date_Time { get; set; } = string.Empty; // ISO

        public string Status { get; set; } = string.Empty;

        public decimal AppointmentFee { get; set; }

        public int SlotDurationMinutes { get; set; }

        public int? QueuePosition { get; set; }

    }

}


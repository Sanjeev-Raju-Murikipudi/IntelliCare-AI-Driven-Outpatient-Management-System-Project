using System;
using System.ComponentModel.DataAnnotations;

namespace IntelliCare.Application.Models
{
    // ✅ Used for creating a new appointment by selecting doctor and time
    public class CreateAppointmentRequest
    {
        [Required(ErrorMessage = "DoctorId is required.")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "PatientId is required.")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "AppointmentDate is required.")]
        public DateTime AppointmentDate { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters.")]
        public string Reason { get; set; } = string.Empty;
    }

    // ✅ Used for rescheduling an existing appointment
    public class RescheduleRequest
    {
        public int OldAppointmentId { get; set; }
        public int NewAppointmentId { get; set; }
        public int PatientId { get; set; }
    }

    // ✅ Used for cancelling an appointment
    public class CancelRequest
    {
        public int AppointmentId { get; set; }
    }
}

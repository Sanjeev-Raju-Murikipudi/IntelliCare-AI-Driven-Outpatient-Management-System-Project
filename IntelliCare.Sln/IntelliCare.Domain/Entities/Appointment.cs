using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IntelliCare.Domain.Enums;

namespace IntelliCare.Domain
{
    [Table("Appointment")]
    public class Appointment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AppointmentID { get; set; }

        [Required]
        public int DoctorID { get; set; }

        public int? PatientID { get; set; }

        [Required]
        public DateTime Date_Time { get; set; }

        [Required]
        public int SlotDurationMinutes { get; set; }

        public int? QueuePosition { get; set; }

        [Required]
        //[StringLength(50)]
        public AppointmentStatus Status { get; set; }

        public string? Reason { get; set; } = string.Empty;
        public decimal AppointmentFee { get; set; }


        [ForeignKey("DoctorID")]
        public virtual Doctor Doctor { get; set; }

        [ForeignKey("PatientID")]
        public virtual Patient Patient { get; set; }
    }
}

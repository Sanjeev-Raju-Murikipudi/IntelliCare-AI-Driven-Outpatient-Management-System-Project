using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntelliCare.Domain
{
    // LLD 2.3: Consultation & Prescription
    [Table("ClinicalRecord")]
    public class ClinicalRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClinicalRecordID { get; set; }

        [Required]
        public int AppointmentID { get; set; }

        public string Notes { get; set; }

        public string Diagnosis { get; set; }

        [StringLength(100)]
        public string Medication { get; set; }

        [StringLength(50)]
        public string PharmacyStatus { get; set; }

        public DateTime? DeliveryETA { get; set; }

        [ForeignKey("AppointmentID")]
        public virtual Appointment Appointment { get; set; }
    }
}

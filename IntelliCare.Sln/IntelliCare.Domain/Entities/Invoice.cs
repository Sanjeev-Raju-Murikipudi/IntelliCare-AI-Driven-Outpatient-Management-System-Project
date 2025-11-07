using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntelliCare.Domain
{
    // LLD 2.4: Billing & Claims
    [Table("Invoice")]
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceID { get; set; }

        [Required]
        public int PatientID { get; set; }

        [Required]
        public int? ClinicalRecordID { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string InsuranceProvider { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(50)]
        public string ClaimStatus { get; set; }

        // --- Navigation Properties ---

        [ForeignKey("PatientID")]
        public virtual Patient Patient { get; set; }

        [ForeignKey("ClinicalRecordID")]
        public virtual ClinicalRecord? ClinicalRecord { get; set; }
    }
}
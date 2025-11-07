using System.ComponentModel.DataAnnotations;

namespace IntelliCare.Application.DTOs
{
    public class CreateInvoiceDTO
    {
        [Required(ErrorMessage = "Clinical Record ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Clinical Record ID must be a positive integer.")]
        public int? ClinicalRecordID { get; set; } 

        [Required(ErrorMessage = "Patient ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Patient ID must be a positive integer.")]
        public int PatientID { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 99999.99, ErrorMessage = "Amount must be greater than zero and realistic.")]
        public decimal Amount { get; set; }

        [MaxLength(100, ErrorMessage = "Insurance Provider cannot exceed 100 characters.")]
      
        public string InsuranceProvider { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        
        [RegularExpression("^(Paid|Pending|Due)$", ErrorMessage = "Status must be Paid, Pending, or Due.")]
        public string Status { get; set; }

        [StringLength(50, ErrorMessage = "Claim Status cannot exceed 50 characters.")]
        [RegularExpression("^(Approved|Pending|Denied|No claim)$", ErrorMessage = "Claim Status is invalid.")]
        public string ClaimStatus { get; set; }
    }
}
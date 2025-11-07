namespace IntelliCare.Application.DTOs
{
    public class InvoiceDTO
    {
        public int InvoiceID { get; set; }
        public int PatientID { get; set; }
        public int? ClinicalRecordID { get; set; }
        public decimal Amount { get; set; }
        public string InsuranceProvider { get; set; }
        public string Status { get; set; }
        public string ClaimStatus { get; set; }
       
    }
}
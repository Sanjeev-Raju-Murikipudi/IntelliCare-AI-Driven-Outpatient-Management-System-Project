

public class PrescriptionDetailDto
{
    public string HospitalName { get; set; }
    public int ClinicalRecordId { get; set; }
    public DateTime PrescriptionDate { get; set; }

    public int PatientId { get; set; } // ✅ Add this
    public string PatientName { get; set; }
    public string DoctorName { get; set; }

    public string DoctorSpecialty { get; set; }
    
   
    public string MedicationDetails { get; set; }
    public string PharmacyStatus { get; set; }
    public DateTime? DeliveryETA { get; set; }
}
using IntelliCare.Domain;

public interface IClinicalRecordRepository
{
   
    Task<ClinicalRecord> AddRecordAsync(ClinicalRecord record);

    
    Task<ClinicalRecord> GetByIdAsync(int recordId);

    
    Task<ClinicalRecord> GetByAppointmentIdAsync(int appointmentId);

    
    Task UpdateAsync(ClinicalRecord record);
    Task<List<ClinicalRecord>> GetAllAsync();
}
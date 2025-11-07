using IntelliCare.Domain;

public interface ISupportDataRepository
{
    Task<SupportData> AddAsync(SupportData data);
    Task<IEnumerable<SupportData>> GetByPatientIdAsync(int patientId);
}

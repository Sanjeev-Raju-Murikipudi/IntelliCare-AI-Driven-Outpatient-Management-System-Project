
using IntelliCare.Application.DTOs;

public interface IConsultationService
{
   
    Task<int> RecordNewConsultationAsync(RecordConsultationDto dto);

   
  
    Task<PrescriptionDetailDto> GenerateEPrescriptionAsync(int appointmentId);


    Task UpdatePrescriptionStatusAsync(int clinicalRecordId, string newStatus, DateTime? newEta);

    Task<List<PrescriptionDetailDto>> GetAllPrescriptionsAsync();

}



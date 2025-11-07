using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliCare.Application.DTOs;

namespace IntelliCare.Application.Interfaces
{
    public interface ISupportDataService
    {
        
        Task<SupportDataDto> UploadDocumentAsync(SupportDataDto dto);

        Task<IEnumerable<SupportDataDto>> GetDocumentsAsync(int patientId);
    }
}

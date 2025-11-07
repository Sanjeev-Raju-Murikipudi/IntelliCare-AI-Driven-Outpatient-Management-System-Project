using IntelliCare.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliCare.Application.Interfaces
{
    public interface IReportService
    {
        // CRUD Operations
        Task<ReportSummaryDto> GenerateReportAsync(ReportRequestDto request);
        Task<ReportSummaryDto> GetReportSummaryAsync(int reportId);

        // New Methods
        Task<IEnumerable<ReportSummaryDto>> GetAllReportSummariesAsync();
        Task<ReportDetailDto> GetReportDetailAsync(int reportId);
        Task<bool> DeleteReportAsync(int reportId);
    }
}
// IntelliCare.Application.Interfaces/IReportRepository.cs

using IntelliCare.Domain; // Assuming your SupportData is in the root Domain namespace
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliCare.Application.Interfaces
{
    public interface IReportRepository
    {
        // Change Task<Report> to Task<SupportData>
        Task<SupportData> GetReportByIdAsync(int reportId);

        // Change AddReportAsync(Report report) to AddReportAsync(SupportData report)
        Task<SupportData> AddReportAsync(SupportData report);

        // Change Task<IEnumerable<Report>> to Task<IEnumerable<SupportData>>
        Task<IEnumerable<SupportData>> GetAllReportsAsync();

        Task<bool> DeleteReportAsync(int reportId);
    }
}
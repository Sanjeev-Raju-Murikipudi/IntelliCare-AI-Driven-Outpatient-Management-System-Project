// IntelliCare.Infrastructure.Persistence.Repositories/ReportRepository.cs (CORRECTED)

using IntelliCare.Application.Interfaces;
using IntelliCare.Domain; // Use the CORRECT namespace for SupportData (likely root Domain)
using IntelliCare.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliCare.Infrastructure.Persistence.Repositories
{
    // The class now correctly implements IReportRepository (which uses SupportData)
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ReportRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Return SupportData
        public async Task<SupportData> GetReportByIdAsync(int reportId)
        {
            // No casting needed since the return type is SupportData
            return await _dbContext.SupportData.FindAsync(reportId);
        }

        // Accepts SupportData
        public async Task<SupportData> AddReportAsync(SupportData report)
        {
            // No casting needed
            _dbContext.SupportData.Add(report);
            await _dbContext.SaveChangesAsync();
            return report;
        }

        // Return IEnumerable<SupportData>
        public async Task<IEnumerable<SupportData>> GetAllReportsAsync()
        {
            // CRITICAL: Filter to ensure only Report data is returned
            return await _dbContext.SupportData
                .Where(s => s.DataType == "REPORT")
                .OrderByDescending(r => r.GeneratedDate)
                .ToListAsync();
        }

        public async Task<bool> DeleteReportAsync(int reportId)
        {
            var reportToDelete = await _dbContext.SupportData.FindAsync(reportId);

            if (reportToDelete == null)
            {
                return false;
            }

            _dbContext.SupportData.Remove(reportToDelete);

            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}
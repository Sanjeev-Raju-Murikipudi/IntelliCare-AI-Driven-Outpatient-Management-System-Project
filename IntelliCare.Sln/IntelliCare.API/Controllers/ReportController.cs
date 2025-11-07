using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntelliCare.WebAPI.Controllers
{
    [Authorize] // <-- UNCOMMENT THIS WHEN READY FOR PRODUCTION/SECURITY TESTING
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        // ===============================================================
        // 1. POST: GENERATE REPORT (CREATE)
        // ===============================================================

        /// <summary>
        /// Generates a new operational or predictive report based on the requested type and parameters.
        /// </summary>
        /// <param name="request">The parameters defining the report (Type, Date Range, optional DoctorID).</param>
        /// <returns>A summary of the newly generated report.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("Generate")]
        [ProducesResponseType(typeof(ReportSummaryDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for report generation request.");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Generating report of type: {ReportType} from {StartDate} to {EndDate}",
                request.ReportType, request.StartDate, request.EndDate);

            try
            {
                var reportSummary = await _reportService.GenerateReportAsync(request);

                if (reportSummary == null)
                {
                    return StatusCode(500, "Failed to generate report due to an internal service error.");
                }

                return Ok(reportSummary);
            }
            catch (System.ArgumentException ex) // Catch the ArgumentException specifically for bad report type
            {
                _logger.LogWarning(ex, "Invalid report type requested: {ReportType}", request.ReportType);
                return BadRequest(ex.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error generating report of type: {ReportType}", request.ReportType);
                return StatusCode(500, $"An error occurred while generating the report: {ex.Message}");
            }
        }

        // ===============================================================
        // 2. GET: RETRIEVE REPORT SUMMARY (READ ONE) - Existing Method
        // ===============================================================

        /// <summary>
        /// Retrieves a previously generated report summary by its unique ID (DataID).
        /// </summary>
        /// <param name="id">The DataID of the saved report.</param>
        /// <returns>The stored Report Summary.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReportSummaryDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetReportSummary(int id)
        {
            _logger.LogInformation("Attempting to retrieve report summary with ID: {ReportId}", id);

            var reportSummary = await _reportService.GetReportSummaryAsync(id);

            if (reportSummary == null)
            {
                _logger.LogWarning("Report with ID {ReportId} not found.", id);
                return NotFound($"Report with ID {id} not found.");
            }

            return Ok(reportSummary);
        }

        // ===============================================================
        // 3. GET: RETRIEVE ALL REPORTS (READ ALL) - NEW
        // ===============================================================

        /// <summary>
        /// Retrieves a list of summaries for ALL generated reports.
        /// This is intended for populating the administrative dashboard's report list.
        /// </summary>
        /// <returns>A list of ReportSummaryDto objects.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("All")]
        [ProducesResponseType(typeof(IEnumerable<ReportSummaryDto>), 200)]
        public async Task<IActionResult> GetAllReports()
        {
            _logger.LogInformation("Fetching summaries for all reports.");
            try
            {
                var summaries = await _reportService.GetAllReportSummariesAsync();
                return Ok(summaries);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all report summaries.");
                return StatusCode(500, "An error occurred while retrieving report summaries.");
            }
        }

        // ===============================================================
        // 4. GET: RETRIEVE REPORT DETAIL (READ DETAIL) - NEW
        // ===============================================================

        /// <summary>
        /// Retrieves the full, detailed content (raw JSON metrics) for a specific report ID.
        /// </summary>
        /// <param name="id">The DataID of the saved report.</param>
        /// <returns>The stored Report Detail DTO.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("Detail/{id}")]
        [ProducesResponseType(typeof(ReportDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetReportDetail(int id)
        {
            _logger.LogInformation("Attempting to retrieve report detail with ID: {ReportId}", id);

            var reportDetail = await _reportService.GetReportDetailAsync(id);

            if (reportDetail == null)
            {
                _logger.LogWarning("Report detail with ID {ReportId} not found.", id);
                return NotFound($"Report detail for ID {id} not found.");
            }

            return Ok(reportDetail);
        }

        // ===============================================================
        // 5. DELETE: DELETE REPORT (DELETE) - NEW
        // ===============================================================

        /// <summary>
        /// Permanently deletes a generated report record from the database.
        /// </summary>
        /// <param name="id">The DataID of the report to delete.</param>
        /// <returns>204 No Content on success.</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteReport(int id)
        {
            _logger.LogInformation("Attempting to delete report with ID: {ReportId}", id);

            var isDeleted = await _reportService.DeleteReportAsync(id);

            if (!isDeleted)
            {
                _logger.LogWarning("Failed to delete report with ID {ReportId}. It may not exist.", id);
                return NotFound($"Report with ID {id} not found or could not be deleted.");
            }

            _logger.LogInformation("Report with ID {ReportId} deleted successfully.", id);
            return NoContent(); // 204 No Content is the standard response for successful DELETE
        }
    }
}
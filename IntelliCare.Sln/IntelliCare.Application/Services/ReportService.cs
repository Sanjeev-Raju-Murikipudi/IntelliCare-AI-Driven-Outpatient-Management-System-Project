using AutoMapper;
using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using IntelliCare.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace IntelliCare.Application.Services
{
    // Assuming SupportData is the entity for Reports for the purpose of GetReportByIdAsync
    using SupportData = SupportData;

    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportService> _logger;
        // Removed 'private static readonly Random Random' since simulations are gone

        public ReportService(
            IReportRepository reportRepository,
            IDoctorRepository doctorRepository,
            IInvoiceRepository invoiceRepository,
            IAppointmentRepository appointmentRepository,
            IMapper mapper,
            ILogger<ReportService> logger)
        {
            _reportRepository = reportRepository;
            _doctorRepository = doctorRepository;
            _invoiceRepository = invoiceRepository;
            _appointmentRepository = appointmentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // =====================================================================
        // MODULE 5: ANALYTICS & OPERATIONAL REPORTING (Service Methods)
        // =====================================================================

        public async Task<ReportSummaryDto> GenerateReportAsync(ReportRequestDto request)
        {
            _logger.LogInformation("Starting report generation for type: {Type}", request.ReportType);

            ReportSummaryDto resultSummary;
            switch (request.ReportType.ToLower())
            {
                case "revenue":
                    // Revenue is now fully calculated using real data
                    resultSummary = await CalculateRevenueAsync(request);
                    break;
                case "utilization":
                    // Utilization is now fully calculated using real data
                    resultSummary = await CalculateUtilizationAsync(request);
                    break;
                case "patientflow":
                    // PatientFlow is now fully calculated using real data
                    resultSummary = await CalculatePatientFlowAsync(request);
                    break;
                case "predictive":
                    // CALLING THE DETERMINISTIC CALCULATION METHOD
                    resultSummary = await CalculatePredictiveAnalysisAsync(request);
                    break;
                default:
                    throw new ArgumentException($"Invalid report type: {request.ReportType}");
            }

            if (resultSummary == null) return null;

            // Serialize complex objects for DB storage
            string metricsJson = JsonSerializer.Serialize(resultSummary.Metrics);
            string detailedJson = JsonSerializer.Serialize(resultSummary.DetailedData);

            // Safely get the non-nullable DoctorId for storage
            int safeDoctorId = request.DoctorId.GetValueOrDefault(0);

            // Create the database entity
            var reportEntity = new SupportData
            {
                Type = request.ReportType,
                GeneratedDate = DateTime.UtcNow,
                DataType = "REPORT",
                DoctorID = safeDoctorId == 0 ? null : safeDoctorId,
                FileName = $"{request.ReportType.ToLower()}_Report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json",
                MetricsJson = metricsJson,
                DetailedDataJson = detailedJson,
            };

            var savedReport = await _reportRepository.AddReportAsync(reportEntity);

            resultSummary.ReportID = savedReport.DataID;
            return resultSummary;
        }

        // ---------------------------------------------------------------------
        // 2. RETRIEVE REPORT SUMMARY (GET BY ID) 
        // ---------------------------------------------------------------------

        public async Task<ReportSummaryDto> GetReportSummaryAsync(int reportId)
        {
            var reportEntity = await _reportRepository.GetReportByIdAsync(reportId);

            if (reportEntity == null) return null;

            var summaryDto = _mapper.Map<ReportSummaryDto>(reportEntity);

            try
            {
                summaryDto.Metrics = JsonSerializer.Deserialize<List<MetricDto>>(reportEntity.MetricsJson);
                summaryDto.DetailedData = JsonSerializer.Deserialize<object>(reportEntity.DetailedDataJson);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize JSON metrics for Report ID {Id}.", reportId);
            }

            return summaryDto;
        }

        // ---------------------------------------------------------------------
        // 3. RETRIEVE ALL REPORT SUMMARIES (GET /ALL)
        // ---------------------------------------------------------------------

        public async Task<IEnumerable<ReportSummaryDto>> GetAllReportSummariesAsync()
        {
            // NOTE: This log indicates we are fetching ALL data, including the detailed portion.
            _logger.LogInformation("Fetching all report summaries, including detailed data.");
            var reportEntities = await _reportRepository.GetAllReportsAsync();

            var summaryList = new List<ReportSummaryDto>();
            foreach (var entity in reportEntities)
            {
                var summaryDto = _mapper.Map<ReportSummaryDto>(entity);
                try
                {
                    summaryDto.Metrics = JsonSerializer.Deserialize<List<MetricDto>>(entity.MetricsJson);
                    // <<< CRITICAL CHANGE: Deserializing DetailedDataJson here >>>
                    summaryDto.DetailedData = JsonSerializer.Deserialize<object>(entity.DetailedDataJson);
                }
                catch (JsonException ex)
                {
                    // Update log message to reflect that detailed data might be missing due to error
                    _logger.LogWarning(ex, "Skipping JSON deserialization for Report ID {Id}. Detailed data may be incomplete.", entity.DataID);
                }
                summaryList.Add(summaryDto);
            }

            return summaryList;
        }

        // ---------------------------------------------------------------------
        // 4. GET REPORT DETAIL (GET /Detail/{id})
        // ---------------------------------------------------------------------
        public async Task<ReportDetailDto> GetReportDetailAsync(int reportId)
        {
            var reportEntity = await _reportRepository.GetReportByIdAsync(reportId);

            if (reportEntity == null) return null;

            return new ReportDetailDto
            {
                ReportID = reportEntity.DataID,
                Type = reportEntity.Type,
                GeneratedDate = reportEntity.GeneratedDate,
                MetricsJson = reportEntity.MetricsJson,
                DetailedDataJson = reportEntity.DetailedDataJson
            };
        }

        // ---------------------------------------------------------------------
        // 5. DELETE REPORT (DELETE /id)
        // ---------------------------------------------------------------------
        public async Task<bool> DeleteReportAsync(int reportId)
        {
            _logger.LogInformation("Attempting to delete report with ID: {ReportId}", reportId);
            return await _reportRepository.DeleteReportAsync(reportId);
        }

        // ---------------------------------------------------------------------
        // === DATABASE LOOKUP HELPER METHOD ===
        // ---------------------------------------------------------------------

        private async Task<string> GetDoctorNameFromDatabaseAsync(int doctorId)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            return doctor != null ? doctor.Name : $"Doctor {doctorId} (Name Not Found)";
        }

        // ---------------------------------------------------------------------
        // === FINAL PRODUCTION REPORT METHOD (REVENUE) ===
        // ---------------------------------------------------------------------

        private async Task<ReportSummaryDto> CalculateRevenueAsync(ReportRequestDto request)
        {
            await Task.Delay(50); // Simulate network latency

            int safeDoctorId = request.DoctorId.GetValueOrDefault(0);

            // 1. Fetch Doctor Name (from DoctorRepository)
            string doctorName = safeDoctorId != 0
                ? await GetDoctorNameFromDatabaseAsync(safeDoctorId)
                : "System Total";

            //TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            //DateTime istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);
            bool dateRangeIsInvalid = request.StartDate >= request.EndDate ;
            //bool isInvalid = request.StartDate >= request.EndDate;


            if (dateRangeIsInvalid)
            {
                string statusMessage = "N/A (Invalid Date Range)";
                _logger.LogWarning("Revenue report returned zero due to invalid date range. Status: {Status}", statusMessage);
                return new ReportSummaryDto
                {
                    Type = "Revenue",
                    GeneratedDate = DateTime.UtcNow,
                    Metrics = new List<MetricDto>
                    {
                        new MetricDto { Label = "Total Revenue", Value = 0.00, Unit = "INR" },
                        new MetricDto { Label = "Appointments Billed", Value = 0, Unit = "count" },
                    },
                    DetailedData = new { TargetDoctor = statusMessage }
                };
            }

            // --- 2. CALCULATE REAL METRICS (NO RANDOM NUMBERS) ---

            // 2a. Get actual revenue from the database (Invoice Repository join)
            double actualTotalRevenue = await _invoiceRepository.GetTotalRevenueForDoctorAsync(
                safeDoctorId,
                request.StartDate,
                request.EndDate
            );

            // 2b. REAL-WORLD FIX: Get the precise count from the Appointment Repository
            int finalAppointments = await _appointmentRepository.GetAppointmentCountForDoctorAsync(
                safeDoctorId,
                request.StartDate,
                request.EndDate
            );


            return new ReportSummaryDto
            {
                Type = "Revenue",
                GeneratedDate = DateTime.UtcNow,
                Metrics = new List<MetricDto>
                {
                    // NOW USES THE REAL DATABASE VALUE
                    new MetricDto { Label = "Total Revenue", Value = Math.Round(actualTotalRevenue, 2), Unit = "INR" },
                    // USES THE ACCURATE DATABASE COUNT
                    new MetricDto { Label = "Appointments Billed", Value = finalAppointments, Unit = "count" },
                },
                DetailedData = new { TargetDoctor = doctorName + (safeDoctorId != 0 ? " (Filtered Result)" : " (System Total)") }
            };
        }

        // ---------------------------------------------------------------------
        // === FINAL PRODUCTION REPORT METHOD (UTILIZATION) ===
        // ---------------------------------------------------------------------
        private async Task<ReportSummaryDto> CalculateUtilizationAsync(ReportRequestDto request)
        {
            await Task.Delay(50);
            int safeDoctorId = request.DoctorId.GetValueOrDefault(0);

            // Get the REAL calculated metrics from the repository
            var utilizationMetrics = await _appointmentRepository.GetDoctorUtilizationMetricsAsync(
                safeDoctorId,
                request.StartDate,
                request.EndDate
            );

            // Get doctor name for the detailed data
            string doctorName = safeDoctorId != 0
                ? await GetDoctorNameFromDatabaseAsync(safeDoctorId)
                : "System Total";

            string detailMessage = doctorName + (safeDoctorId == 0
                ? " (System Wide)"
                : $" (Filtered Avg Utilization: {utilizationMetrics.UtilizationRate}%)");

            return new ReportSummaryDto
            {
                Type = "Utilization",
                GeneratedDate = DateTime.UtcNow,
                Metrics = new List<MetricDto>
                {
                    // Now uses real calculated data from the repository
                    new MetricDto {
                        Label = "Doctor Utilization Rate",
                        Value = utilizationMetrics.UtilizationRate,
                        Unit = "%"
                    },
                    new MetricDto {
                        Label = "No-Show Rate",
                        Value = utilizationMetrics.NoShowRate,
                        Unit = "%"
                    },
                },
                DetailedData = new { TargetDoctor = detailMessage }
            };
        }

        // ---------------------------------------------------------------------
        // === FINAL PRODUCTION REPORT METHOD (PATIENT FLOW) ===
        // ---------------------------------------------------------------------
        private async Task<ReportSummaryDto> CalculatePatientFlowAsync(ReportRequestDto request)
        {
            await Task.Delay(50);
            int safeDoctorId = request.DoctorId.GetValueOrDefault(0);

            // Get the REAL calculated metrics (using the deterministic proxy from the repository)
            var flowMetrics = await _appointmentRepository.GetPatientFlowMetricsAsync(
                safeDoctorId,
                request.StartDate,
                request.EndDate
            );

            // Get doctor name for the detailed data
            string doctorName = safeDoctorId != 0
                ? await GetDoctorNameFromDatabaseAsync(safeDoctorId)
                : "System Total";

            // Use the real, calculated values
            double avgWaitTime = flowMetrics.AverageWaitTimeMinutes;
            int peakHour = flowMetrics.PeakFlowHour;

            // The detail message is updated to reflect that the 'wait time' is a scheduled duration proxy
            string detailMessage = doctorName + (safeDoctorId == 0
                ? " (System Total)"
                : $" (Filtered Avg {avgWaitTime} min scheduled duration)");

            return new ReportSummaryDto
            {
                Type = "PatientFlow",
                GeneratedDate = DateTime.UtcNow,
                Metrics = new List<MetricDto>
                {
                    // Now uses real calculated data from the repository
                    new MetricDto { Label = "Avg Scheduled Duration", Value = avgWaitTime, Unit = "min" },
                    new MetricDto { Label = "Peak Flow Hour", Value = peakHour, Unit = "hour" },
                },
                DetailedData = new { TargetDoctor = detailMessage }
            };
        }

        // ---------------------------------------------------------------------
        // === FINAL PRODUCTION REPORT METHOD (PREDICTIVE) ===
        // ---------------------------------------------------------------------
        private async Task<ReportSummaryDto> CalculatePredictiveAnalysisAsync(ReportRequestDto request)
        {
            await Task.Delay(50);
            int safeDoctorId = request.DoctorId.GetValueOrDefault(0);

            // Get the REAL calculated metrics from the repository
            var predictiveMetrics = await _appointmentRepository.GetPredictiveMetricsAsync(safeDoctorId);

            // Get doctor name for the detailed data
            string doctorName = safeDoctorId != 0
                ? await GetDoctorNameFromDatabaseAsync(safeDoctorId)
                : "System Total";

            string detailMessage = doctorName + (safeDoctorId == 0
                ? " (Based on System Trends)"
                : $" (Based on Doctor {doctorName} Trends)");

            return new ReportSummaryDto
            {
                Type = "Predictive",
                GeneratedDate = DateTime.UtcNow,
                Metrics = new List<MetricDto>
                {
                    new MetricDto {
                        Label = "Projected Next Month Visits",
                        Value = predictiveMetrics.ProjectedVisits,
                        Unit = "count"
                    },
                    new MetricDto {
                        Label = "Resource Need (Next 30 Days)",
                        Value = predictiveMetrics.ResourceNeed,
                        Unit = "new doctors"
                    },
                },
                // The detailed data message now shows the data source (historical trend)
                DetailedData = new { ForecastBasis = detailMessage }
            };
        }
    }
}

using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;

// Import the service, interfaces, and domain models
using IntelliCare.Application.Services;
using IntelliCare.Application.Interfaces;
using IntelliCare.Domain;
using IntelliCare.Application.DTOs; // Already present
//using IntelliCare.Domain.DTOs; // FIX: Add this if UtilizationMetricsDto is in this namespace

namespace IntelliCare.Tests.Application.Services
{
    [TestFixture]
    public class ReportServiceTests
    {
        // Service under test
        private ReportService _service;

        // Mocks for all dependencies
        private Mock<IReportRepository> _mockReportRepo;
        private Mock<IDoctorRepository> _mockDoctorRepo;
        private Mock<IInvoiceRepository> _mockInvoiceRepo;
        private Mock<IAppointmentRepository> _mockAppointmentRepo;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<ReportService>> _mockLogger;

        // Shared test data
        private ReportRequestDto _defaultRequest;
        private int _testDoctorId = 5;
        private DateTime _startDate = new DateTime(2023, 1, 1);
        private DateTime _endDate = new DateTime(2023, 1, 31);

        [SetUp]
        public void Setup()
        {
            // Initialize all mocks
            _mockReportRepo = new Mock<IReportRepository>();
            _mockDoctorRepo = new Mock<IDoctorRepository>();
            _mockInvoiceRepo = new Mock<IInvoiceRepository>();
            _mockAppointmentRepo = new Mock<IAppointmentRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ReportService>>();

            // Instantiate the service
            _service = new ReportService(
                _mockReportRepo.Object,
                _mockDoctorRepo.Object,
                _mockInvoiceRepo.Object,
                _mockAppointmentRepo.Object,
                _mockMapper.Object,
                _mockLogger.Object);

            // Setup default request
            _defaultRequest = new ReportRequestDto
            {
                ReportType = "revenue",
                DoctorId = _testDoctorId,
                StartDate = _startDate,
                EndDate = _endDate
            };

            // Setup common Doctor mock
            _mockDoctorRepo.Setup(r => r.GetByIdAsync(_testDoctorId))
                .ReturnsAsync(new Doctor { DoctorId = _testDoctorId, Name = "Dr. Test" });
        }

        #region GenerateReportAsync Tests

        [Test]
        public async Task GenerateReportAsync_RevenueType_CallsRevenueCalculatorAndSavesReport()
        {
            double expectedRevenue = 1500.00;
            int expectedCount = 10;

            _mockInvoiceRepo.Setup(r => r.GetTotalRevenueForDoctorAsync(_testDoctorId, _startDate, _endDate))
                .ReturnsAsync(expectedRevenue);
            _mockAppointmentRepo.Setup(r => r.GetAppointmentCountForDoctorAsync(_testDoctorId, _startDate, _endDate))
                .ReturnsAsync(expectedCount);

            var savedEntity = new SupportData { DataID = 101, Type = "revenue" };
            _mockReportRepo.Setup(r => r.AddReportAsync(It.IsAny<SupportData>())).ReturnsAsync(savedEntity);

            // ARRANGEr
            var request = new ReportRequestDto
            {
                ReportType = "revenue",
                DoctorId = _defaultRequest.DoctorId,
                StartDate = _defaultRequest.StartDate,
                EndDate = _defaultRequest.EndDate
            };
            // ACT
            var result = await _service.GenerateReportAsync(request);

            // ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ReportID, Is.EqualTo(101));
            Assert.That(result.Type, Is.EqualTo("Revenue"));
            Assert.That(result.Metrics.First(m => m.Label == "Total Revenue").Value, Is.EqualTo(expectedRevenue));

            _mockReportRepo.Verify(r => r.AddReportAsync(It.Is<SupportData>(e =>
                e.Type == "revenue" &&
                e.DoctorID == _testDoctorId &&
                e.MetricsJson.Contains(expectedRevenue.ToString())
            )), Times.Once);
        }

        [Test]
        public void GenerateReportAsync_InvalidReportType_ThrowsArgumentException()
        {
            // ARRANGE
            var invalidRequest = new ReportRequestDto
            {
                ReportType = "unknown",
                DoctorId = _defaultRequest.DoctorId,
                StartDate = _defaultRequest.StartDate,
                EndDate = _defaultRequest.EndDate
            };

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateReportAsync(invalidRequest));

            _mockReportRepo.Verify(r => r.AddReportAsync(It.IsAny<SupportData>()), Times.Never);
        }

        [Test]
        public async Task GenerateReportAsync_NullDoctorId_SetsDoctorIdToNullInEntity()
        {
            // ARRANGE
            var request = new ReportRequestDto
            {
                ReportType = "revenue",
                DoctorId = null,
                StartDate = _defaultRequest.StartDate,
                EndDate = _defaultRequest.EndDate
            };

            _mockInvoiceRepo.Setup(r => r.GetTotalRevenueForDoctorAsync(0, _startDate, _endDate)).ReturnsAsync(100.0);
            _mockAppointmentRepo.Setup(r => r.GetAppointmentCountForDoctorAsync(0, _startDate, _endDate)).ReturnsAsync(5);

            var savedEntity = new SupportData { DataID = 101 };
            _mockReportRepo.Setup(r => r.AddReportAsync(It.IsAny<SupportData>())).ReturnsAsync(savedEntity);

            // ACT
            await _service.GenerateReportAsync(request);

            // ASSERT
            _mockReportRepo.Verify(r => r.AddReportAsync(It.Is<SupportData>(e =>
                e.DoctorID == null
            )), Times.Once);

            _mockDoctorRepo.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region Calculate*Async Method Tests

        [Test]
        public async Task CalculateRevenueAsync_CallsInvoiceAndAppointmentReposWithCorrectParameters()
        {
            // ARRANGE
            var request = new ReportRequestDto
            {
                ReportType = "revenue",
                DoctorId = _defaultRequest.DoctorId,
                StartDate = _defaultRequest.StartDate,
                EndDate = _defaultRequest.EndDate
            };

            _mockInvoiceRepo.Setup(r => r.GetTotalRevenueForDoctorAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(1000.00);
            _mockAppointmentRepo.Setup(r => r.GetAppointmentCountForDoctorAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(10);
            _mockReportRepo.Setup(r => r.AddReportAsync(It.IsAny<SupportData>())).ReturnsAsync(new SupportData());

            // ACT
            await _service.GenerateReportAsync(request);

            // ASSERT
            _mockInvoiceRepo.Verify(r => r.GetTotalRevenueForDoctorAsync(
                _testDoctorId,
                _startDate,
                _endDate),
            Times.Once);

            _mockAppointmentRepo.Verify(r => r.GetAppointmentCountForDoctorAsync(
                _testDoctorId,
                _startDate,
                _endDate),
            Times.Once);
        }

        [Test]
        public async Task CalculateUtilizationAsync_CallsAppointmentRepoWithCorrectParameters()
        {
            // ARRANGE
            var request = new ReportRequestDto
            {
                ReportType = "utilization",
                DoctorId = _defaultRequest.DoctorId,
                StartDate = _defaultRequest.StartDate,
                EndDate = _defaultRequest.EndDate
            };

            _mockAppointmentRepo.Setup(r => r.GetDoctorUtilizationMetricsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new DoctorUtilizationMetricsDto { UtilizationRate = 50.0, NoShowRate = 10.0 });
            _mockReportRepo.Setup(r => r.AddReportAsync(It.IsAny<SupportData>())).ReturnsAsync(new SupportData());

            // ACT
            await _service.GenerateReportAsync(request);

            // ASSERT
            _mockAppointmentRepo.Verify(r => r.GetDoctorUtilizationMetricsAsync(
                _testDoctorId,
                _startDate,
                _endDate),
            Times.Once);
        }

        #endregion

        #region GetReportSummaryAsync Tests

        [Test]

        public async Task GetReportSummaryAsync_JsonDeserializationFails_LogsErrorAndReturnsPartiallyPopulatedDto()
        {
            // ARRANGE
            int reportId = 2;
            var reportEntity = new SupportData
            {
                DataID = reportId,
                MetricsJson = "INVALID JSON", // Malformed JSON
                DetailedDataJson = "{}",
                Type = "Revenue"
            };

            // IMPORTANT: Assume mapper returns a DTO where Metrics is an empty list if not explicitly set to null
            var baseDto = new ReportSummaryDto { ReportID = reportId, Type = "Revenue", Metrics = new List<MetricDto>() };
            _mockMapper.Setup(m => m.Map<ReportSummaryDto>(reportEntity)).Returns(baseDto);

            _mockReportRepo.Setup(r => r.GetReportByIdAsync(reportId)).ReturnsAsync(reportEntity);

            // ACT
            var result = await _service.GetReportSummaryAsync(reportId);

            // ASSERT
            Assert.That(result, Is.Not.Null);

            // FIX APPLIED: Check if the collection is either Null or Empty
            Assert.That(result.Metrics, Is.Empty.Or.Null, "Metrics should be null or empty after deserialization fails.");

            // Verify error was logged (using the corrected Moq syntax from the previous step)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Failed to deserialize JSON metrics for Report ID {reportId}")),
                    It.IsAny<JsonException>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        #endregion

        #region DeleteReportAsync Tests

        [Test]
        public async Task DeleteReportAsync_CallsRepositoryDeleteMethod()
        {
            // ARRANGE
            int reportId = 4;
            _mockReportRepo.Setup(r => r.DeleteReportAsync(reportId)).ReturnsAsync(true);

            // ACT
            var result = await _service.DeleteReportAsync(reportId);

            // ASSERT
            Assert.That(result, Is.True);
            _mockReportRepo.Verify(r => r.DeleteReportAsync(reportId), Times.Once);

            // FIX 3: Corrected logger verification to avoid optional argument issues.
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Attempting to delete report with ID: {reportId}")),
                    null, // Use null for the exception argument when no exception is expected
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        #endregion
    }
}
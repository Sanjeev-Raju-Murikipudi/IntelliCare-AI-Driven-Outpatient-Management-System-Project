using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.Common; // Required for Job
using Hangfire.States; // Required for IState

// Include necessary namespaces from your application
using IntelliCare.Application.Services;
using IntelliCare.Application.Interfaces;
using IntelliCare.Domain;
using IntelliCare.Application.DTOs;
using IntelliCare.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;


namespace IntelliCare.Tests.Application.Services
{
    [TestFixture]
    public class AppointmentServiceTests
    {
        // 1. Service instance and mock declarations
        private AppointmentService _service;
        private Mock<IAppointmentRepository> _mockRepo;
        private Mock<IUserService> _mockUserService;
        private Mock<IPatientService> _mockPatientService;
        private Mock<IEmailSender> _mockEmailSender;
        private Mock<IQueueNotifier> _mockNotifier;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<IBackgroundJobClient> _mockBackgroundJobs;
        private Mock<IWhatsAppSender> _mockWhatsappSender;
        private Mock<ILogger<AppointmentService>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            // Initialization remains the same
            _mockRepo = new Mock<IAppointmentRepository>();
            _mockUserService = new Mock<IUserService>();
            _mockPatientService = new Mock<IPatientService>();
            _mockEmailSender = new Mock<IEmailSender>();
            _mockNotifier = new Mock<IQueueNotifier>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockBackgroundJobs = new Mock<IBackgroundJobClient>();
            _mockWhatsappSender = new Mock<IWhatsAppSender>();
            _mockLogger = new Mock<ILogger<AppointmentService>>();

            // HttpContext setup
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, "Admin") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };

            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            _mockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(principal);

            // Instantiate the service
            _service = new AppointmentService(
                _mockRepo.Object,
                _mockPatientService.Object,
                _mockUserService.Object,
                _mockEmailSender.Object,
                _mockNotifier.Object,
                _mockHttpContextAccessor.Object,
                _mockLogger.Object,
                _mockWhatsappSender.Object,
                _mockBackgroundJobs.Object);
        }

        #region Helper Methods

        // Helper method to create a PatientDto that satisfies IsProfileComplete
        private PatientDto GetCompletePatientDto(int patientId, string username = "TestPatient")
        {
            return new PatientDto
            {
                PatientId = patientId,
                Name = username,
                // These fields must be set to ensure IsProfileComplete evaluates to true
                ContactEmail = $"{username}@test.com",
                DOB = DateTime.Today.AddYears(-30),
                PhoneNumber = "1234567890", // Must be 10 digits
                MedicalHistory = "None",
                InsuranceDetails = "Aetna",
                FullName = "Test Patient Full Name"
            };
        }

        // Helper to create a complete User object for notification
        private User GetCompleteUser(int? doctorId, int? patientId, string mobileNumber)
        {
            return new User
            {
                DoctorID = doctorId,
                PatientID = patientId,
                ContactEmail = $"user{patientId ?? doctorId}@test.com",
                MobileNumber = mobileNumber
            };
        }

        #endregion

        // -------------------------------------------------------------

        #region BookAsync Tests



        [Test]
        public void BookAsync_PatientProfileIncomplete_ThrowsInvalidOperationException()
        {
            // ARRANGE
            int patientId = 1;
            var bookDto = new BookAppointmentDto { Date_Time = DateTime.Now.AddDays(1) };
            var incompletePatientDto = new PatientDto { PatientId = patientId };

            _mockUserService.Setup(x => x.GetLoggedInPatientId()).Returns(patientId);
            _mockPatientService.Setup(x => x.GetPatientByIdAsync(patientId)).ReturnsAsync(incompletePatientDto);

            // ACT & ASSERT
            Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.BookAsync(bookDto),
                "Please complete your profile before booking."
            );

            _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
        }




        #endregion

        // -------------------------------------------------------------

        #region CreateSlotsAsync Tests

        [Test]
        public async Task CreateSlotsAsync_ValidAdminRequest_CreatesCorrectNumberOfSlots()
        {
            // ARRANGE
            var createDto = new CreateSlotDto
            {
                DoctorID = 20,
                Date = DateTime.Today.AddDays(7),
                StartTime = "09:00",
                EndTime = "11:00",
                IntervalMinutes = 30,
                Fee = 750
            };

            // ACT
            await _service.CreateSlotsAsync(createDto);

            // ASSERT
            _mockRepo.Verify(x => x.AddRangeAsync(It.Is<IEnumerable<Appointment>>(list =>
                list.Count() == 4 &&
                list.All(a => a.DoctorID == 20)
            )), Times.Once);
        }

        [Test]
        public void CreateSlotsAsync_NonAdminUser_ThrowsUnauthorizedAccessException()
        {
            // ARRANGE: Override the default Admin setup with a Patient role
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, "Patient") };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _mockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(principal);

            var createDto = new CreateSlotDto { DoctorID = 20, Date = DateTime.Today, StartTime = "09:00", EndTime = "10:00", IntervalMinutes = 60 };

            // ACT & ASSERT
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateSlotsAsync(createDto));

            _mockRepo.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<Appointment>>()), Times.Never);
        }

        #endregion

        // -------------------------------------------------------------

        #region CancelAsync Tests



        [Test]
        public void CancelAsync_AppointmentNotFound_ThrowsUnauthorizedAccessException()
        {
            // ARRANGE
            int patientId = 1;
            var cancelDto = new CancelAppointmentDto { AppointmentID = 999 };

            _mockUserService.Setup(x => x.GetLoggedInPatientId()).Returns(patientId);
            _mockRepo.Setup(x => x.GetAppointmentByIdAsync(cancelDto.AppointmentID)).ReturnsAsync((Appointment)null);

            // ACT & ASSERT
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CancelAsync(cancelDto));

            _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<Appointment>()), Times.Never);
        }
        #endregion

        // -------------------------------------------------------------

        #region CompleteAppointmentAsync Tests

        [Test]
        public async Task CompleteAppointmentAsync_NextPatientExists_NotifiesNextPatient()
        {
            // ARRANGE
            int doctorId = 10;
            var currentSlot = new Appointment { AppointmentID = 100, DoctorID = doctorId, Status = AppointmentStatus.Booked, Date_Time = DateTime.Today.AddHours(10), PatientID = 1 };
            var nextSlot = new Appointment { AppointmentID = 101, DoctorID = doctorId, Status = AppointmentStatus.Booked, Date_Time = DateTime.Today.AddHours(10).AddMinutes(30), PatientID = 2 };

            var nextPatientUser = GetCompleteUser(null, 2, "9998887777");
            var doctor = new Doctor { DoctorId = doctorId, Name = "Dr. House" };

            _mockRepo.Setup(x => x.GetAppointmentByIdAsync(currentSlot.AppointmentID)).ReturnsAsync(currentSlot);

            _mockRepo.Setup(x => x.GetNextPatientInQueueAsync(doctorId, currentSlot.Date_Time)).ReturnsAsync(nextSlot);
            _mockRepo.Setup(x => x.GetUserByPatientIdAsync(nextSlot.PatientID.Value)).ReturnsAsync(nextPatientUser);
            _mockRepo.Setup(x => x.GetDoctorByIdAsync(doctorId)).ReturnsAsync(doctor);

            // ACT
            await _service.CompleteAppointmentAsync(currentSlot.AppointmentID);

            // ASSERT
            _mockRepo.Verify(x => x.UpdateAsync(It.Is<Appointment>(a =>
                a.AppointmentID == 100 &&
                a.Status == AppointmentStatus.Completed)), Times.Once);

            // **CRITICAL FIX:** Verify the underlying IBackgroundJobClient.Create method.
            _mockBackgroundJobs.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Exactly(2));

            _mockNotifier.Verify(x => x.NotifyQueueUpdateAsync(doctorId), Times.Once);
        }

        [Test]
        public async Task CompleteAppointmentAsync_NoNextPatient_OnlyUpdatesStatus()
        {
            // ARRANGE
            int doctorId = 10;
            var currentSlot = new Appointment { AppointmentID = 100, DoctorID = doctorId, Status = AppointmentStatus.Booked, Date_Time = DateTime.Today.AddHours(10), PatientID = 1 };

            _mockRepo.Setup(x => x.GetAppointmentByIdAsync(currentSlot.AppointmentID)).ReturnsAsync(currentSlot);

            _mockRepo.Setup(x => x.GetNextPatientInQueueAsync(doctorId, currentSlot.Date_Time)).ReturnsAsync((Appointment)null);

            // ACT
            await _service.CompleteAppointmentAsync(currentSlot.AppointmentID);

            // ASSERT
            _mockRepo.Verify(x => x.UpdateAsync(It.Is<Appointment>(a =>
                a.AppointmentID == 100 &&
                a.Status == AppointmentStatus.Completed)), Times.Once);


            // **CRITICAL FIX:** Verify the underlying IBackgroundJobClient.Create method with Times.Never.
            _mockBackgroundJobs.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Never);

            _mockNotifier.Verify(x => x.NotifyQueueUpdateAsync(doctorId), Times.Once);
        }

        #endregion
    }
}
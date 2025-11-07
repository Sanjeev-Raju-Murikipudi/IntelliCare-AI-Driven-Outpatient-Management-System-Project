using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;

// Import your service, DTOs, interfaces, and domain models
using IntelliCare.Application.Services;
using IntelliCare.Application.Interfaces;
using IntelliCare.Domain;
using IntelliCare.Domain.Enums;
using IntelliCare.Application.DTOs;

namespace IntelliCare.Test
{
    [TestFixture]
    public class ConsultationServiceTests
    {
        // Service under test
        private ConsultationService _service;

        // Mocks for all dependencies
        private Mock<IClinicalRecordRepository> _mockRecordRepo;
        private Mock<IPatientRepository> _mockPatientRepo;
        private Mock<IDoctorRepository> _mockDoctorRepo;
        private Mock<IAppointmentRepository> _mockAppointmentRepo;

        [SetUp]
        public void Setup()
        {
            // Initialize mocks
            _mockRecordRepo = new Mock<IClinicalRecordRepository>();
            _mockPatientRepo = new Mock<IPatientRepository>();
            _mockDoctorRepo = new Mock<IDoctorRepository>();
            _mockAppointmentRepo = new Mock<IAppointmentRepository>();

            // Initialize the service with the mocked dependencies
            _service = new ConsultationService(
                _mockRecordRepo.Object,
                _mockPatientRepo.Object,
                _mockDoctorRepo.Object,
                _mockAppointmentRepo.Object);
        }

        // --- Helper Methods to Create Test Data ---

        private ClinicalRecord GetTestClinicalRecord(int recordId, int appointmentId, string medication, string status)
        {
            return new ClinicalRecord
            {
                ClinicalRecordID = recordId,
                AppointmentID = appointmentId,
                Notes = "Routine checkup.",
                Diagnosis = "Common Cold",
                Medication = medication,
                PharmacyStatus = status,
                DeliveryETA = null
            };
        }

        private Appointment GetTestAppointment(int appointmentId, int? patientId, int doctorId, AppointmentStatus status = AppointmentStatus.Completed)
        {
            return new Appointment
            {
                AppointmentID = appointmentId,
                PatientID = patientId,
                DoctorID = doctorId,
                Date_Time = DateTime.Now.AddHours(-1),
                Status = status
            };
        }

        private RecordConsultationDto GetTestRecordConsultationDto(int appointmentId, string medication = "Medication X")
        {
            return new RecordConsultationDto
            {
                AppointmentId = appointmentId,
                Notes = "Test Notes",
                Diagnosis = "Test Diagnosis",
                Medication = medication
            };
        }

        // ------------------------------------------------------------------
        // ## 1. RecordNewConsultationAsync Tests
        // ------------------------------------------------------------------

        [Test]
        public async Task RecordNewConsultationAsync_ValidDataWithMedication_SetsPendingStatusAndReturnsId()
        {
            // ARRANGE
            var inputDto = GetTestRecordConsultationDto(101, "Paracetamol 500mg");
            var bookedAppointment = GetTestAppointment(101, 1, 10, AppointmentStatus.Completed);
            var expectedRecord = GetTestClinicalRecord(201, 101, inputDto.Medication, "Pending Submission");

            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(101)).ReturnsAsync(bookedAppointment);
            _mockRecordRepo.Setup(r => r.AddRecordAsync(It.IsAny<ClinicalRecord>())).ReturnsAsync(expectedRecord);

            // ACT
            var resultId = await _service.RecordNewConsultationAsync(inputDto);

            // ASSERT
            Assert.That(resultId, Is.EqualTo(201));
            _mockRecordRepo.Verify(r => r.AddRecordAsync(
                It.Is<ClinicalRecord>(r => r.Medication == "Paracetamol 500mg" && r.PharmacyStatus == "Pending Submission")),
                Times.Once);
        }

        [Test]
        public async Task RecordNewConsultationAsync_ValidDataWithoutMedication_SetsNAStatus()
        {
            // ARRANGE
            var inputDto = GetTestRecordConsultationDto(102, null); // No medication
            var bookedAppointment = GetTestAppointment(102, 1, 10, AppointmentStatus.Completed);
            var expectedRecord = GetTestClinicalRecord(202, 102, null, "N/A"); // Expected record for verification

            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(102)).ReturnsAsync(bookedAppointment);
            _mockRecordRepo.Setup(r => r.AddRecordAsync(It.IsAny<ClinicalRecord>())).ReturnsAsync(expectedRecord);

            // ACT
            var resultId = await _service.RecordNewConsultationAsync(inputDto);

            // ASSERT
            Assert.That(resultId, Is.EqualTo(202));
            _mockRecordRepo.Verify(r => r.AddRecordAsync(
                It.Is<ClinicalRecord>(r => r.Medication == null && r.PharmacyStatus == "N/A")),
                Times.Once);
        }

        [Test]
        public void RecordNewConsultationAsync_AppointmentNotFound_ThrowsInvalidOperationException()
        {
            // ARRANGE
            var inputDto = GetTestRecordConsultationDto(999);
            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(999)).ReturnsAsync((Appointment)null);

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RecordNewConsultationAsync(inputDto));

            Assert.That(ex.Message, Is.EqualTo("Appointment not found."));
            _mockRecordRepo.Verify(r => r.AddRecordAsync(It.IsAny<ClinicalRecord>()), Times.Never);
        }

        [Test]
        public void RecordNewConsultationAsync_AppointmentHasNoPatient_ThrowsInvalidOperationException()
        {
            // ARRANGE
            var inputDto = GetTestRecordConsultationDto(103);
            var unbookedAppointment = GetTestAppointment(103, null, 10, AppointmentStatus.Completed);
            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(103)).ReturnsAsync(unbookedAppointment);

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.RecordNewConsultationAsync(inputDto));

            Assert.That(ex.Message, Is.EqualTo("Cannot record consultation: No patient has booked this appointment."));
            _mockRecordRepo.Verify(r => r.AddRecordAsync(It.IsAny<ClinicalRecord>()), Times.Never);
        }

        // ------------------------------------------------------------------
        // ## 2. UpdatePrescriptionStatusAsync Tests
        // ------------------------------------------------------------------

        [Test]
        public async Task UpdatePrescriptionStatusAsync_ValidRecord_UpdatesStatusAndETA()
        {
            // ARRANGE
            var existingRecord = GetTestClinicalRecord(300, 150, "Ibuprofen", "Pending Submission");
            var newStatus = "Delivered";
            var newEta = DateTime.Now.AddMinutes(5);

            _mockRecordRepo.Setup(r => r.GetByIdAsync(300)).ReturnsAsync(existingRecord);

            // ACT
            await _service.UpdatePrescriptionStatusAsync(300, newStatus, newEta);

            // ASSERT
            _mockRecordRepo.Verify(r => r.UpdateAsync(
                It.Is<ClinicalRecord>(r =>
                    r.ClinicalRecordID == 300 &&
                    r.PharmacyStatus == newStatus &&
                    r.DeliveryETA == newEta)),
                Times.Once);
        }

        [Test]
        public void UpdatePrescriptionStatusAsync_RecordNotFound_ThrowsException()
        {
            // ARRANGE
            _mockRecordRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ClinicalRecord)null);

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<Exception>(() =>
                _service.UpdatePrescriptionStatusAsync(999, "Processing", DateTime.Now));

            Assert.That(ex.Message, Is.EqualTo("Clinical Record ID 999 not found."));
            _mockRecordRepo.Verify(r => r.UpdateAsync(It.IsAny<ClinicalRecord>()), Times.Never);
        }

        // ------------------------------------------------------------------
        // ## 3. GenerateEPrescriptionAsync Tests
        // ------------------------------------------------------------------

        [Test]
        public async Task GenerateEPrescriptionAsync_ValidData_ReturnsDetailedDto()
        {
            // ARRANGE
            int appointmentId = 50, patientId = 1, doctorId = 10;
            var record = GetTestClinicalRecord(500, appointmentId, "Amoxicillin 250mg", "Shipped");
            var appointment = GetTestAppointment(appointmentId, patientId, doctorId, AppointmentStatus.Completed);
            var patient = new Patient { PatientId = patientId, FullName = "Alice Smith" };
            var doctor = new Doctor { DoctorId = doctorId, Name = "Dr. Jane Doe", Specialization = "Pediatrics" };

            // Mock dependencies chain
            _mockRecordRepo.Setup(r => r.GetByAppointmentIdAsync(appointmentId)).ReturnsAsync(record);
            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(appointmentId)).ReturnsAsync(appointment);
            _mockPatientRepo.Setup(r => r.GetByIdAsync(patientId)).ReturnsAsync(patient);
            _mockDoctorRepo.Setup(r => r.GetByIdAsync(doctorId)).ReturnsAsync(doctor);

            // ACT
            var result = await _service.GenerateEPrescriptionAsync(appointmentId);

            // ASSERT
            Assert.That(result.HospitalName, Is.EqualTo("Intellicare"));
            Assert.That(result.PatientName, Is.EqualTo("Alice Smith"));
            Assert.That(result.DoctorName, Is.EqualTo("Dr. Jane Doe"));
            Assert.That(result.DoctorSpecialty, Is.EqualTo("Pediatrics"));
            Assert.That(result.MedicationDetails, Is.EqualTo("Amoxicillin 250mg"));
            Assert.That(result.PharmacyStatus, Is.EqualTo("Shipped"));
            Assert.That(result.PrescriptionDate.Date, Is.EqualTo(DateTime.Now.Date));
        }

        [Test]
        public void GenerateEPrescriptionAsync_ClinicalRecordNotFound_ThrowsException()
        {
            // ARRANGE
            int appointmentId = 51;
            _mockRecordRepo.Setup(r => r.GetByAppointmentIdAsync(appointmentId)).ReturnsAsync((ClinicalRecord)null);

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<Exception>(() =>
                _service.GenerateEPrescriptionAsync(appointmentId));

            Assert.That(ex.Message, Is.EqualTo("Clinical Record for Appointment ID 51 not found."));
        }

        [Test]
        public void GenerateEPrescriptionAsync_AssociatedAppointmentNotFound_ThrowsException()
        {
            // ARRANGE
            int appointmentId = 52;
            var record = GetTestClinicalRecord(502, appointmentId, "Med", "N/A");

            _mockRecordRepo.Setup(r => r.GetByAppointmentIdAsync(appointmentId)).ReturnsAsync(record);
            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(appointmentId)).ReturnsAsync((Appointment)null);

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<Exception>(() =>
                _service.GenerateEPrescriptionAsync(appointmentId));

            Assert.That(ex.Message, Is.EqualTo($"Associated Appointment ID {appointmentId} not found."));
        }

        [Test]
        public void GenerateEPrescriptionAsync_AppointmentHasNoPatientID_ThrowsException()
        {
            // ARRANGE
            int appointmentId = 53;
            var record = GetTestClinicalRecord(503, appointmentId, "Med", "N/A");
            var appointment = new Appointment { AppointmentID = appointmentId, PatientID = null, DoctorID = 10 };

            _mockRecordRepo.Setup(r => r.GetByAppointmentIdAsync(appointmentId)).ReturnsAsync(record);
            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(appointmentId)).ReturnsAsync(appointment);

            // ACT & ASSERT
            var ex = Assert.ThrowsAsync<Exception>(() =>
                _service.GenerateEPrescriptionAsync(appointmentId));

            Assert.That(ex.Message, Is.EqualTo($"Appointment ID {appointmentId} is not booked to a patient."));
        }

        [Test]
        public async Task GenerateEPrescriptionAsync_PatientOrDoctorNotFound_UsesDefaultNames()
        {
            // ARRANGE
            int appointmentId = 54, patientId = 3, doctorId = 13;
            var record = GetTestClinicalRecord(504, appointmentId, "Aspirin", "N/A");
            var appointment = GetTestAppointment(appointmentId, patientId, doctorId, AppointmentStatus.Completed);

            // Return null for patient and doctor
            _mockRecordRepo.Setup(r => r.GetByAppointmentIdAsync(appointmentId)).ReturnsAsync(record);
            _mockAppointmentRepo.Setup(r => r.GetAppointmentByIdAsync(appointmentId)).ReturnsAsync(appointment);
            _mockPatientRepo.Setup(r => r.GetByIdAsync(patientId)).ReturnsAsync((Patient)null);
            _mockDoctorRepo.Setup(r => r.GetByIdAsync(doctorId)).ReturnsAsync((Doctor)null);

            // ACT
            var result = await _service.GenerateEPrescriptionAsync(appointmentId);

            // ASSERT
            Assert.That(result.PatientName, Is.EqualTo("Unknown Patient"));
            Assert.That(result.DoctorName, Is.EqualTo("Unknown Doctor"));
            Assert.That(result.DoctorSpecialty, Is.EqualTo("Unknown Specialty"));
        }
    }
}
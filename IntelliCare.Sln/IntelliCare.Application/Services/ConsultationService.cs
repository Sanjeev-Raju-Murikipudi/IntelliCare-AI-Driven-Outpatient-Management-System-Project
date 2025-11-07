// File: IntelliCare.Application/Services/ConsultationService.cs

using System;

using System.Threading.Tasks;

using IntelliCare.Application.DTOs;

using IntelliCare.Application.Interfaces;

using IntelliCare.Domain;

using IntelliCare.Domain.Enums;

namespace IntelliCare.Application.Services

{

    public class ConsultationService : IConsultationService

    {

        // Private fields for all required repositories

        private readonly IClinicalRecordRepository _clinicalRecordRepo;

        private readonly IPatientRepository _patientRepo;

        private readonly IDoctorRepository _doctorRepo;

        private readonly IAppointmentRepository _appointmentRepo;

        // Constructor: Inject all necessary repositories

        public ConsultationService(

            IClinicalRecordRepository clinicalRecordRepo,

            IPatientRepository patientRepo,

            IDoctorRepository doctorRepo,

            IAppointmentRepository appointmentRepo)

        {

            _clinicalRecordRepo = clinicalRecordRepo;

            _patientRepo = patientRepo;

            _doctorRepo = doctorRepo;

            _appointmentRepo = appointmentRepo;

        }

        // 1. Record a new consultation

        public async Task<int> RecordNewConsultationAsync(RecordConsultationDto dto)

        {

            var appointment = await _appointmentRepo.GetAppointmentByIdAsync(dto.AppointmentId);

            if (appointment == null)

                throw new InvalidOperationException("Appointment not found.");

            if (appointment.PatientID == null)

                throw new InvalidOperationException("Cannot record consultation: No patient has booked this appointment.");


            var record = new ClinicalRecord

            {

                AppointmentID = dto.AppointmentId,

                Notes = dto.Notes,

                Diagnosis = dto.Diagnosis,

                Medication = dto.Medication,

                PharmacyStatus = string.IsNullOrEmpty(dto.Medication) ? "N/A" : "Pending Submission",

                DeliveryETA = null

            };

            var result = await _clinicalRecordRepo.AddRecordAsync(record);

            return result.ClinicalRecordID;

        }

        // 2. Update prescription status

        public async Task UpdatePrescriptionStatusAsync(int clinicalRecordId, string newStatus, DateTime? newEta)

        {

            var record = await _clinicalRecordRepo.GetByIdAsync(clinicalRecordId);

            if (record == null)

                throw new Exception($"Clinical Record ID {clinicalRecordId} not found.");

            record.PharmacyStatus = newStatus;

            record.DeliveryETA = newEta;

            await _clinicalRecordRepo.UpdateAsync(record);

        }

        // 3. Generate e-prescription

        public async Task<PrescriptionDetailDto> GenerateEPrescriptionAsync(int appointmentId)

        {

            var record = await _clinicalRecordRepo.GetByAppointmentIdAsync(appointmentId);

            if (record == null)

                throw new Exception($"Clinical Record for Appointment ID {appointmentId} not found.");

            var appointment = await _appointmentRepo.GetAppointmentByIdAsync(record.AppointmentID);

            if (appointment == null)

                throw new Exception($"Associated Appointment ID {record.AppointmentID} not found.");

            if (appointment.PatientID == null)

                throw new Exception($"Appointment ID {appointment.AppointmentID} is not booked to a patient.");

            var patient = await _patientRepo.GetByIdAsync(appointment.PatientID.Value);

            var doctor = await _doctorRepo.GetByIdAsync(appointment.DoctorID);

            var dto = new PrescriptionDetailDto

            {

                HospitalName = "Intellicare",

                ClinicalRecordId = record.ClinicalRecordID,

                PrescriptionDate = DateTime.Now,

                PatientName = patient?.FullName ?? "Unknown Patient",

                DoctorName = doctor?.Name ?? "Unknown Doctor",

                DoctorSpecialty = doctor?.Specialization ?? "Unknown Specialty",

                MedicationDetails = record.Medication,

                PharmacyStatus = record.PharmacyStatus,

                DeliveryETA = record.DeliveryETA

            };

            return dto;

        }






        public async Task<List<PrescriptionDetailDto>> GetAllPrescriptionsAsync()
        {
            var records = await _clinicalRecordRepo.GetAllAsync(); // Assuming this method exists

            var result = new List<PrescriptionDetailDto>();

            foreach (var record in records)
            {
                var appointment = await _appointmentRepo.GetAppointmentByIdAsync(record.AppointmentID);
                if (appointment == null || appointment.PatientID == null) continue;

                var patient = await _patientRepo.GetByIdAsync(appointment.PatientID.Value);
                var doctor = await _doctorRepo.GetByIdAsync(appointment.DoctorID);

                result.Add(new PrescriptionDetailDto
                {
                    HospitalName = "Intellicare",
                    ClinicalRecordId = record.ClinicalRecordID,
                    //PrescriptionDate = record.CreatedAt, // or DateTime.Now if not available
                    PatientName = patient?.FullName ?? "Unknown Patient",
                    DoctorName = doctor?.Name ?? "Unknown Doctor",
                    DoctorSpecialty = doctor?.Specialization ?? "Unknown Specialty",
                    MedicationDetails = record.Medication,
                    PharmacyStatus = record.PharmacyStatus,
                    DeliveryETA = record.DeliveryETA
                });
            }

            return result;
        }

    }

}


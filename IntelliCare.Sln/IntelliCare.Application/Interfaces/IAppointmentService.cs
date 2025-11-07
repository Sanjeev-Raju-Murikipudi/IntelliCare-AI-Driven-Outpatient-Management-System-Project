using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntelliCare.Application.DTOs;

namespace IntelliCare.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<List<AppointmentDto>> GetAppointmentsForDoctorAsync(string doctorUsername);
        //Task<PatientDto> GetPatientDetailsByAppointmentIdAsync(int appointmentId);
        Task MarkAppointmentCompletedAsync(int appointmentId);

        Task<PatientSummaryDto> GetPatientSummaryByAppointmentIdAsync(int appointmentId);

        Task<List<AppointmentDto>> GetBookedAppointmentsForDoctorByDateAsync(string doctorUsername, DateTime date);

        // Task<List<AppointmentDto>> GetAllAppointmentsForTodayAsync();


      

            Task<List<AppointmentDto>> GetAllAppointmentsForTodayAsync();


        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();


        Task<List<MyAppointmentDto>> GetAppointmentsByPatientIdAsync(int patientId);


    }
}

using IntelliCare.Application.DTOs;
using IntelliCare.Domain;

public interface IAppointmentRepository
{
   

    Task<Appointment?> GetAvailableSlotAsync(int doctorId, DateTime dateTime);
    Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);
    Task<List<Appointment>> GetDoctorAvailabilityAsync(int doctorId);
    Task<List<Appointment>> GetQueueAsync(int doctorId, DateTime date);
    Task<List<Appointment>> GetAppointmentsInBlockAsync(int doctorId, DateTime blockStart);
    Task<Appointment?> GetNextAvailableSlotAsync(int doctorId, DateTime after);
    Task<Appointment?> GetNextPatientInQueueAsync(int doctorId, DateTime blockStart);
    Task<Doctor?> GetDoctorByIdAsync(int doctorId);
    Task<Patient?> GetPatientByIdAsync(int patientId);
    Task<bool> SlotExistsAsync(int doctorId, DateTime dateTime);

    Task<Appointment?> GetByIdAsync(int appointmentId);

    Task<List<Appointment>> GetAppointmentsForPatientInRangeAsync(int patientId, DateTime from, DateTime to);

    Task<List<Appointment>> GetAppointmentsStartingBeforeAsync(DateTime now);
    Task<List<Appointment>> GetDoctorQueueByDateAsync(int doctorId, DateTime date);

    Task<List<Appointment>> GetFutureAvailableAppointmentsAsync(int doctorId);

    Task<List<Appointment>> GetAppointmentsEndingBeforeAsync(DateTime now);

    Task<Appointment?> GetNextBookedAppointmentAsync(int doctorId, DateTime date, DateTime afterTime);

    Task<List<Appointment>> GetAvailableSlotsForDayAsync(int doctorId, DateTime date);

    Task<List<Appointment>> GetUpcomingAppointmentsForPatientAsync(int patientId, DateTime fromDate, DateTime toDate);

    Task<List<Appointment>> GetAppointmentsForDoctorOnDateAsync(int doctorId, DateTime date);

    Task<List<Appointment>> GetAppointmentsByPatientOnDateAsync(int patientId, DateTime date);

    Task AddRangeAsync(IEnumerable<Appointment> slots);

    Task<List<Appointment>> GetBookedAppointmentsForDoctorByDateAsync(int doctorId, DateTime date);

    Task<User?> GetUserByUsernameAsync(string username);

    Task<User?> GetUserByDoctorIdAsync(int doctorId);
    Task<User?> GetUserByPatientIdAsync(int patientId);

    Task AddAsync(Appointment appointment);
    Task UpdateAsync(Appointment appointment);

    Task<DoctorUtilizationMetricsDto> GetDoctorUtilizationMetricsAsync(int doctorId, DateTime startDate, DateTime endDate);


    Task<int> GetAppointmentCountForDoctorAsync(int doctorId, DateTime start, DateTime end);

    /// <summary>
    /// Calculates complex patient flow metrics (Avg Wait Time Proxy, Peak Hour).
    /// </summary>
    Task<PatientFlowMetricsDto> GetPatientFlowMetricsAsync(int doctorId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Calculates predictive metrics (Projected Visits, Resource Need) based on historical data.
    /// </summary>
    Task<PredictiveMetricsDto> GetPredictiveMetricsAsync(int doctorId, int lookbackDays = 90);
    Task<bool> DoctorExistsAsync(int doctorId);


    Task<List<Appointment>> GetAllBookedAppointmentsByDateAsync(DateTime date); // ✅ New



    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<List<Appointment>> GetAppointmentsByPatientIdAsync(int patientId);


}


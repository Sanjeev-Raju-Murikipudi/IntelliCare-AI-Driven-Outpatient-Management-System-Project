using IntelliCare.Application.DTOs;
using IntelliCare.Application.Interfaces;
using IntelliCare.Domain;
using IntelliCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IntelliCare.Infrastructure.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment?> GetAvailableSlotAsync(int doctorId, DateTime dateTime)

        {

            var targetDate = dateTime.Date;

            var targetTime = dateTime.TimeOfDay;

            // Define bookable statuses

            var bookableStatuses = new[]

            {

        AppointmentStatus.Available,

        AppointmentStatus.ReopenedFromCancellation,

        AppointmentStatus.ReopenedFromReschedule

    };

            // Find matching slot

            var slot = await _context.Appointments

                .Where(a =>

                    a.DoctorID == doctorId &&

                    a.PatientID == null &&

                    bookableStatuses.Contains(a.Status) &&

                    a.Date_Time.Date == targetDate &&

                    a.Date_Time.TimeOfDay == targetTime)

                .FirstOrDefaultAsync();

            return slot; // ✅ Return null if not found — let service layer handle messaging

        }



        public async Task AddRangeAsync(IEnumerable<Appointment> slots)
        {
            await _context.Appointments.AddRangeAsync(slots);
            await _context.SaveChangesAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId);
        }
        public async Task<List<Appointment>> GetDoctorAvailabilityAsync(int doctorId)
        {
            return await _context.Appointments
                .Where(a => a.DoctorID == doctorId &&
                            a.PatientID == null &&
                            a.Status == AppointmentStatus.Available)
                .OrderBy(a => a.Date_Time)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetBookedAppointmentsForDoctorByDateAsync(int doctorId, DateTime date)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Where(a =>
                    a.DoctorID == doctorId &&
                    a.Date_Time.Date == date.Date &&
                    (a.Status == AppointmentStatus.Booked || a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.InProgress
 ))
                .ToListAsync();
        }




        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<List<Appointment>> GetUpcomingAppointmentsForPatientAsync(int patientId, DateTime fromDate, DateTime toDate)
        {
            return await _context.Appointments
                .Where(a => a.PatientID == patientId &&
                            a.Date_Time.Date >= fromDate.Date &&
                            a.Date_Time.Date <= toDate.Date &&
                            a.Status == AppointmentStatus.Booked)
                .ToListAsync();
        }


        public async Task<List<Appointment>> GetAvailableSlotsForDayAsync(int doctorId, DateTime date)
        {
            return await _context.Appointments
                .Where(a => a.DoctorID == doctorId &&
                            a.Date_Time.Date == date.Date &&
                            a.Status == AppointmentStatus.Available &&
                            a.PatientID == null)
                .OrderBy(a => a.Date_Time)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsByPatientOnDateAsync(int patientId, DateTime date)
        {
            return await _context.Appointments
                .Where(a => a.PatientID == patientId &&
                            a.Date_Time.Date == date.Date)
                .ToListAsync();
        }
        public async Task<bool> DoctorExistsAsync(int doctorId)

        {

            return await _context.Doctors.AnyAsync(d => d.DoctorId == doctorId);

        }

        public async Task<List<Appointment>> GetFutureAvailableAppointmentsAsync(int doctorId)

        {

            var now = DateTime.Now;

            return await _context.Appointments

                .Include(a => a.Doctor)

                .Where(a => a.DoctorID == doctorId &&

                            a.Date_Time > now &&

                            (a.Status == AppointmentStatus.Available ||

                             a.Status == AppointmentStatus.ReopenedFromCancellation ||

                             a.Status == AppointmentStatus.ReopenedFromReschedule))

                .OrderBy(a => a.Date_Time)

                .ToListAsync();

        }



        public async Task<List<Appointment>> GetAppointmentsStartingBeforeAsync(DateTime now)
        {
            return await _context.Appointments
                .Where(a => a.Date_Time <= now &&
                            a.Date_Time.AddMinutes(a.SlotDurationMinutes) > now &&
                            a.Status == AppointmentStatus.Booked)
                .ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Patient) // optional if you need patient info
                .FirstOrDefaultAsync(a => a.AppointmentID == appointmentId);
        }

        public async Task<List<Appointment>> GetDoctorQueueByDateAsync(int doctorId, DateTime date)
        {
            return await _context.Appointments
                .Where(a => a.DoctorID == doctorId &&
                            a.Date_Time.Date == date.Date &&
                            a.Status != AppointmentStatus.Completed &&
                            a.PatientID != null)
                .OrderBy(a => a.Date_Time)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetAppointmentsForPatientInRangeAsync(int patientId, DateTime from, DateTime to)
        {
            return await _context.Appointments
                .Where(a => a.PatientID == patientId &&
                            a.Date_Time >= from &&
                            a.Date_Time <= to &&
                            a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();
        }


        public async Task<List<Appointment>> GetAppointmentsForDoctorOnDateAsync(int doctorId, DateTime date)
        {
            return await _context.Appointments
                .Where(a => a.DoctorID == doctorId &&
                            a.Date_Time.Date == date.Date)
                .OrderBy(a => a.Date_Time)
                .ToListAsync();
        }


        public async Task<List<Appointment>> GetQueueAsync(int doctorId, DateTime date)
        {
            return await _context.Appointments
                .Where(a => a.DoctorID == doctorId &&
                            a.Date_Time.Date == date.Date &&
                            a.Status == AppointmentStatus.Booked)
                .OrderBy(a => a.QueuePosition)
                .ToListAsync();
        }

        public async Task<User?> GetUserByDoctorIdAsync(int doctorId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.DoctorID == doctorId);
        }

        public async Task<User?> GetUserByPatientIdAsync(int patientId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.PatientID == patientId);
        }

        public async Task<List<Appointment>> GetAppointmentsInBlockAsync(int doctorId, DateTime blockStart)
        {
            return await _context.Appointments
                .Where(a => a.DoctorID == doctorId &&
                            a.Date_Time == blockStart &&
                            a.Status == AppointmentStatus.Booked)
                .ToListAsync();
        }

        public async Task<bool> SlotExistsAsync(int doctorId, DateTime dateTime)
        {
            return await _context.Appointments
                .AnyAsync(a => a.DoctorID == doctorId && a.Date_Time == dateTime);
        }

        public async Task<Appointment?> GetNextAvailableSlotAsync(int doctorId, DateTime after)
        {
            return await _context.Appointments
                .Where(a => a.DoctorID == doctorId &&
                            a.Date_Time > after &&
                            a.PatientID == null &&
                            a.Status == AppointmentStatus.Available)
                .OrderBy(a => a.Date_Time)
                .FirstOrDefaultAsync();
        }

        public async Task<Appointment?> GetNextPatientInQueueAsync(int doctorId, DateTime blockStart)
        {
            return await _context.Appointments
                .Where(a => a.DoctorID == doctorId &&
                            a.Date_Time == blockStart &&
                            a.Status == AppointmentStatus.Booked)
                .OrderBy(a => a.QueuePosition)
                .FirstOrDefaultAsync();
        }

        public async Task<Doctor> GetDoctorByIdAsync(int doctorId)
        {
            return await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == doctorId);
        }


        public async Task<Patient?> GetPatientByIdAsync(int patientId)
        {
            return await _context.Patients.FindAsync(patientId);
        }

        public async Task AddAsync(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
        }


        public async Task<Appointment?> GetNextBookedAppointmentAsync(int doctorId, DateTime date, DateTime afterTime)
        {
            return await _context.Appointments
                .Include(a => a.Patient) // Load Patient only
                .Where(a => a.DoctorID == doctorId &&
                            a.Date_Time.Date == date.Date &&
                            a.Date_Time > afterTime &&
                            a.Status == AppointmentStatus.Booked)
                .OrderBy(a => a.Date_Time)
                .FirstOrDefaultAsync();
        }


        public async Task<List<Appointment>> GetAppointmentsEndingBeforeAsync(DateTime now)
        {
            return await _context.Appointments
                .Where(a => a.Date_Time.AddMinutes(a.SlotDurationMinutes) <= now &&
                            (a.Status == AppointmentStatus.Booked || a.Status == AppointmentStatus.InProgress))
                .ToListAsync();
        }

        public async Task UpdateAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        // Services/ReportService.cs

        public async Task<DoctorUtilizationMetricsDto> GetDoctorUtilizationMetricsAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
           
            var queryStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var queryEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc).AddSeconds(1);

        
            var appointments = await _context.Appointments
                .Where(a => a.DoctorID == doctorId)
                .Where(a => a.Date_Time >= queryStart && a.Date_Time < queryEnd)
                .ToListAsync();

            if (!appointments.Any())
            {
                return new DoctorUtilizationMetricsDto { UtilizationRate = 0, NoShowRate = 0 };
            }

            // --- No-Show Rate Calculation ---

            int totalBooked = appointments.Count;
            // NOTE: Ensure AppointmentStatus.NoShow is the correct enum value.
            int totalNoShow = appointments.Count(a => a.Status == AppointmentStatus.NoShow);

            // Handle division by zero edge case safely
            double noShowRate = (totalBooked > 0) ? (totalNoShow / (double)totalBooked) * 100.0 : 0.0;

            // --- Utilization Rate Calculation ---

            // 1. Calculate total scheduled/booked time (excluding cancellations)
            double totalBookedMinutes = appointments
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .Sum(a => a.SlotDurationMinutes);

            // 2. Realistic Calculation for Total Available Time:
            //    A. Find the unique days the doctor actually had appointments during the period.
            //       (This avoids counting unavailable days, weekends, etc., simplifying the "real-world" problem)
            var uniqueAppointmentDates = appointments
                // The Date_Time is an IST value with Kind=Utc; extract the Date part.
                .Select(a => a.Date_Time.Date)
                .Distinct()
                .ToList();

            int daysWithAppointments = uniqueAppointmentDates.Count;

            //    B. Look up the doctor's available minutes per day. 
            //       Since a real schedule is not available, we assume a standard 8-hour shift (480 min) 
            //       ONLY for the days the doctor was scheduled.
            const int StandardDailyAvailableMinutes = 480; // 8 hours (480 minutes)

            // Total available time is the assumed daily shift multiplied by the number of days 
            // the doctor actually saw patients/was scheduled.
            double totalAvailableMinutes = daysWithAppointments * StandardDailyAvailableMinutes;

            // If no days had available time (e.g., totalBookedMinutes is 0), avoid division by zero.
            double utilizationRate = (totalAvailableMinutes > 0)
                ? (totalBookedMinutes / totalAvailableMinutes) * 100.0
                : 0.0;

            if (utilizationRate > 100.0) utilizationRate = 100.0; // Cap at 100%

            return new DoctorUtilizationMetricsDto
            {
                UtilizationRate = Math.Round(utilizationRate, 1),
                NoShowRate = Math.Round(noShowRate, 1)
            };
        }



        public async Task<int> GetAppointmentCountForDoctorAsync(int doctorId, DateTime start, DateTime end)
        {
            // Ensure consistent time handling with the revenue method
            var queryStart = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            // Use AddMilliseconds(1) for a cleaner exclusive end range if possible, but keep AddSeconds(1) if consistent with other code
            var queryEnd = DateTime.SpecifyKind(end, DateTimeKind.Utc).AddSeconds(1);

            // --- Start the LINQ Query with explicit joins ---
            var billedAppointmentCount = await (
                from appointment in _context.Appointments

                    // 1. Initial Filtering by Doctor and Time Range
                where (doctorId == 0 || appointment.DoctorID == doctorId) // Handles DoctorID filter
                    && appointment.Date_Time >= queryStart
                    && appointment.Date_Time < queryEnd // Use < queryEnd if you added the second, or <= if you didn't

                // 2. Join Appointment to ClinicalRecord (Must exist to be billed)
                join clinicalRecord in _context.ClinicalRecords
                    on appointment.AppointmentID equals clinicalRecord.AppointmentID

                // 3. Join ClinicalRecord to Invoice (Must exist to be Billed)
                join invoice in _context.Invoices
                    on clinicalRecord.ClinicalRecordID equals invoice.ClinicalRecordID

                // 4. Filter by Paid Status (Only count appointments that generated true revenue)
                where invoice.Status.ToLower() == "paid"

                // 5. Select the Appointment object for counting
                select appointment
            )
            .Distinct() // Crucial: Ensures each unique appointment is counted only once
            .CountAsync();

            return billedAppointmentCount;
        }


        //public async Task<int> GetAppointmentCountForDoctorAsync(int doctorId, DateTime start, DateTime end)
        //{
        //    var queryStart = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        //    var queryEnd = DateTime.SpecifyKind(end, DateTimeKind.Utc).AddSeconds(1);

        //    // Start with all appointments
        //    var query = _context.Appointments.AsQueryable();

        //    // Filter by DoctorID ONLY if it is a specific Doctor (i.e., not 0 for System Total)
        //    if (doctorId != 0)
        //    {
        //        query = query.Where(a => a.DoctorID == doctorId);
        //    }

        //    // Apply the necessary filters for the REVENUE report
        //    return await query
        //        // 1. Filter by Date Range
        //        .Where(a => a.Date_Time >= queryStart && a.Date_Time <= queryEnd)

        //        // 2. Filter by Status = 1 (Assuming 1 means Completed/Billed)
        //        .Where(a => a.Status == AppointmentStatus.Booked || a.Status == AppointmentStatus.Completed)

        //        // 3. Execute the count
        //        .CountAsync();
        //}






        // Services/ReportService.cs

        // Services/ReportService.cs

        public async Task<PatientFlowMetricsDto> GetPatientFlowMetricsAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            // FIX 1: Date Handling (IST-as-UTC hack)
            var queryStart = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            var queryEnd = DateTime.SpecifyKind(endDate, DateTimeKind.Utc).AddSeconds(1);

            // Define the IST TimeZone for accurate Peak Hour grouping
            TimeZoneInfo istZone;
            try
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }

            // Fetch relevant appointments
            var appointments = await _context.Appointments
                .Where(a => a.DoctorID == doctorId)
                .Where(a => a.Date_Time >= queryStart && a.Date_Time < queryEnd)
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();

            if (!appointments.Any())
            {
                return new PatientFlowMetricsDto { AverageWaitTimeMinutes = 0, PeakFlowHour = 0 };
            }

            // Calculate Average Metric (Assuming SlotDuration is used for this metric)
            double avgMetric = appointments.Average(a => a.SlotDurationMinutes);

            // FIX 2: Peak Hour Logic (Convert to Local Time before grouping)
            var peakHour = appointments
    .Select(a =>
    {
        // CRITICAL FIX: The Date_Time is an IST value with Kind=Utc.
        // 1. Reset the kind to UNICODE to signal it is the "source" time zone.
        var istValueWithNoKind = DateTime.SpecifyKind(a.Date_Time, DateTimeKind.Unspecified);

        // 2. Convert the 'Unspecified' time, treating it as if it's already IST,
        // to a new DateTime with the correct IST Kind.
        return TimeZoneInfo.ConvertTime(istValueWithNoKind, istZone);
    })
    .GroupBy(localTime => localTime.Hour) // Group by the local IST hour (0 to 23)
    .OrderByDescending(g => g.Count())
    .Select(g => g.Key)
    .FirstOrDefault();

            return new PatientFlowMetricsDto
            {
                AverageWaitTimeMinutes = Math.Round(avgMetric, 1),
                PeakFlowHour = peakHour
            };
        }


        // Services/ReportService.cs

        public async Task<PredictiveMetricsDto> GetPredictiveMetricsAsync(int doctorId, int lookbackDays = 30)
        {
            // Constants for clarity and easy modification
            const double DailyDoctorCapacityMinutes = 480.0; // 8 hours * 60 minutes
            const double MaxUtilizationTarget = 0.90;        // Target utilization is 90%
            const double ProjectionPeriodDays = 30.0;
            const double GrowthFactor = 1.05;                // 5% growth

            // FIX 1: Use the system's current time (localtime) and specify it as UTC 
            // to match the database's IST-as-UTC convention.
            var endDateLocal = DateTime.Now;
            var startDateLocal = endDateLocal.AddDays(-lookbackDays);

            // Convert to the kind required for the database filter (IST-as-UTC hack)
            var queryEndDate = DateTime.SpecifyKind(endDateLocal, DateTimeKind.Utc);
            var queryStartDate = DateTime.SpecifyKind(startDateLocal, DateTimeKind.Utc);

            var historicalAppointments = await _context.Appointments
                // 1. Filter by Doctor or System-wide
                .Where(a => a.DoctorID == doctorId || doctorId == 0)
                // 2. Filter by Corrected Historical Time Window
                .Where(a => a.Date_Time >= queryStartDate && a.Date_Time <= queryEndDate)

                // 3. Keep filtering for appointments that consumed a slot (exclude cancelled)
                .Where(a => a.Status != AppointmentStatus.Cancelled)

                .ToListAsync();

            if (!historicalAppointments.Any())
            {
                return new PredictiveMetricsDto { ProjectedVisits = 0, ResourceNeed = 0.0 };
            }

            // --- Projected Visits Calculation ---
            int totalHistoricalAppointments = historicalAppointments.Count;
            double avgDailyVisits = totalHistoricalAppointments / (double)lookbackDays;

            // Extrapolate daily average for 30 days with a 5% growth factor
            int projectedVisits = (int)Math.Round(avgDailyVisits * ProjectionPeriodDays * GrowthFactor);

            // --- Resource Need Calculation ---

            // 1. Total minutes booked (excluding cancelled)
            double totalBookedMinutes = historicalAppointments.Sum(a => a.SlotDurationMinutes);

            // 2. Calculate Actual Utilization Rate based on the historical period (lookbackDays)
            double totalAvailableMinutes = lookbackDays * DailyDoctorCapacityMinutes;
            double actualUtilizationRate = (totalBookedMinutes / totalAvailableMinutes);

            double resourceNeed = 0.0;

            // Check if the current utilization exceeds the target
            if (actualUtilizationRate > MaxUtilizationTarget)
            {
                // Average minutes of demand per day
                double avgDailyDemandMinutes = totalBookedMinutes / (double)lookbackDays;

                // Target available minutes per day (90% capacity)
                double targetDailyCapacityMinutes = DailyDoctorCapacityMinutes * MaxUtilizationTarget;

                // Minutes over the target per day
                double minutesOverTargetPerDay = avgDailyDemandMinutes - targetDailyCapacityMinutes;

                // Total excess demand minutes over the projection period
                double totalExcessDemandMinutes = minutesOverTargetPerDay * ProjectionPeriodDays * GrowthFactor;

                // Capacity provided by one new doctor over the projection period
                double capacityOfOneNewDoctor = DailyDoctorCapacityMinutes * ProjectionPeriodDays;

                // The fraction of a new doctor needed to cover the total excess demand
                resourceNeed = totalExcessDemandMinutes / capacityOfOneNewDoctor;
            }

            return new PredictiveMetricsDto
            {
                ProjectedVisits = projectedVisits,
                // Round the resource need to 1 decimal place. Only report a positive need.
                ResourceNeed = Math.Round(resourceNeed > 0.1 ? resourceNeed : 0.0, 1)
            };
        }

        public async Task<List<Appointment>> GetAllBookedAppointmentsByDateAsync(DateTime date)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.Status == AppointmentStatus.Booked && a.Date_Time.Date == date.Date)
                .ToListAsync();
        }




        public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.Date_Time)
                .ToListAsync();
        }




        public async Task<List<Appointment>> GetAppointmentsByPatientIdAsync(int patientId)

        {

            return await _context.Appointments

                .Include(a => a.Doctor)

                .Where(a => a.PatientID == patientId)

                .OrderByDescending(a => a.Date_Time)

                .ToListAsync();

        }





    }




}

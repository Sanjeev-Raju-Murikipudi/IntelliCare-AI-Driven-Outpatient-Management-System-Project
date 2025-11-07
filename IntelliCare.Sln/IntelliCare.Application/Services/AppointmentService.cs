using Hangfire;
using IntelliCare.Application.DTOs;
using IntelliCare.Application.Helpers;
using IntelliCare.Application.Interfaces;
using IntelliCare.Domain;
using IntelliCare.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IntelliCare.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repo;
        private readonly IPatientService _patientService;
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;
        private readonly IQueueNotifier _notifier;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AppointmentService> _logger;
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly IWhatsAppSender _whatsappSender;


        public AppointmentService(
            IAppointmentRepository repo,
            IPatientService patientService,
            IUserService userService,
            IEmailSender emailSender,
            IQueueNotifier notifier,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AppointmentService> logger,
            IWhatsAppSender whatsappSender,
            IBackgroundJobClient backgroundJobs)
        {
            _repo = repo;
            _patientService = patientService;
            _userService = userService;
            _emailSender = emailSender;
            _notifier = notifier;
            _httpContextAccessor = httpContextAccessor;
            _backgroundJobs = backgroundJobs;
            _whatsappSender = whatsappSender;
            _logger = logger;
        }

        public async Task<BookAppointmentResult> BookAsync(BookAppointmentDto dto)

        {

            _logger.LogInformation("Booking appointment for patient ID {PatientId} on {DateTime}", _userService.GetLoggedInPatientId(), dto.Date_Time);

            var patientId = _userService.GetLoggedInPatientId();

            var patient = await _patientService.GetPatientByIdAsync(patientId);

            var patientUser = await _repo.GetUserByPatientIdAsync(patientId);

            if (patient == null || !patient.IsProfileComplete)

            {

                _logger.LogWarning("Patient ID {PatientId} has incomplete profile", patientId);

                throw new InvalidOperationException("Please complete your profile before booking.");

            }

            if (dto.Date_Time < DateTime.Now)

            {

                _logger.LogWarning("Attempted to book appointment in the past: {DateTime}", dto.Date_Time);

                throw new ArgumentException("Appointment must be scheduled in the future.");

            }

            var today = DateTime.Today;

            var maxDate = today.AddDays(15);

            if (dto.Date_Time.Date > maxDate)

            {

                _logger.LogWarning("Appointment date {DateTime} exceeds max allowed date {MaxDate}", dto.Date_Time, maxDate);

                throw new ArgumentException($"Appointments can only be booked within the next 15 days. Please select a date before {maxDate:dd MMM yyyy}.");

            }

            bool isEmergency = dto.Reason?.IndexOf("emergency", StringComparison.OrdinalIgnoreCase) >= 0;

            var now = DateTime.Now;

            var fifteenDaysAgo = now.AddDays(-15);

            var fifteenDaysAhead = now.AddDays(15);

            var activeAppointments = await _repo.GetAppointmentsForPatientInRangeAsync(patientId, fifteenDaysAgo, fifteenDaysAhead);

            if (activeAppointments.Any(a => a.Status != AppointmentStatus.Cancelled) && !isEmergency)

            {

                var latest = activeAppointments.OrderByDescending(a => a.Date_Time).First();

                var validUntil = latest.Date_Time.AddDays(15);

                var remainingDays = (validUntil - now).Days;

                throw new InvalidOperationException(

                    $"You already have an active appointment on {latest.Date_Time:dd MMM yyyy hh:mm tt}. " +

                    $"You can revisit until {validUntil:dd MMM yyyy} ({remainingDays} days remaining). Emergency reason required to book again.");

            }

            var slot = await _repo.GetAvailableSlotAsync(dto.DoctorID, dto.Date_Time);

            // Make sure cancelled future slots are reusable

            if (slot != null && slot.Status == AppointmentStatus.Cancelled && slot.Date_Time > DateTime.Now)

            {

                slot.PatientID = null;

                slot.Status = AppointmentStatus.Available;

                slot.QueuePosition = 0;

                slot.Reason = null;

                slot.AppointmentFee = dto.Fee ?? 500;

                await _repo.UpdateAsync(slot);

            }

            // Re-fetch slot after potential reset

            slot = await _repo.GetAvailableSlotAsync(dto.DoctorID, dto.Date_Time);

            if (slot == null || slot.PatientID != null ||
                !(slot.Status == AppointmentStatus.Available ||
                  slot.Status == AppointmentStatus.ReopenedFromCancellation ||
                  slot.Status == AppointmentStatus.ReopenedFromReschedule))
            {
                var availableSlots = await _repo.GetAvailableSlotsForDayAsync(dto.DoctorID, dto.Date_Time.Date);

                var futureAvailableSlots = availableSlots
                    .Where(s => s.Date_Time > DateTime.Now && s.PatientID == null &&
                        (s.Status == AppointmentStatus.Available ||
                         s.Status == AppointmentStatus.ReopenedFromCancellation ||
                         s.Status == AppointmentStatus.ReopenedFromReschedule))
                    .OrderBy(s => s.Date_Time)
                    .ToList();

                if (!futureAvailableSlots.Any())

                    throw new InvalidOperationException($"No future available slots for doctor {dto.DoctorID} on {dto.Date_Time:dd MMM yyyy}.");

                var suggestions = futureAvailableSlots.Select(s => s.Date_Time.ToString("HH:mm")).ToList();

                string suggestionText = string.Join(", ", suggestions);

                throw new InvalidOperationException($"No available slot at {dto.Date_Time:dd MMM yyyy HH:mm}. Found {futureAvailableSlots.Count} future slots for doctor {dto.DoctorID}. Suggested times: {suggestionText}");

            }

            var appointmentsToday = await _repo.GetAppointmentsForDoctorOnDateAsync(dto.DoctorID, slot.Date_Time.Date);

            int queuePosition;

            if (isEmergency)

            {

                queuePosition = 1;

                foreach (var appt in appointmentsToday.Where(a => a.Status == AppointmentStatus.Booked))

                {

                    appt.QueuePosition += 1;

                    await _repo.UpdateAsync(appt);

                }

            }

            else

            {

                queuePosition = appointmentsToday.Count(a => a.Status == AppointmentStatus.Booked) + 1;

            }

            slot.PatientID = patientId;

            slot.Reason = dto.Reason;

            slot.Status = AppointmentStatus.Booked;

            slot.QueuePosition = queuePosition;

            slot.AppointmentFee = dto.Fee ?? 500;

            await _repo.UpdateAsync(slot);

            var doctorUser = await _repo.GetUserByDoctorIdAsync(dto.DoctorID);

            var doctor = await _repo.GetDoctorByIdAsync(dto.DoctorID);

            // Email to Doctor

            _backgroundJobs.Enqueue(() =>

                _emailSender.SendEmailAsync(

                    doctorUser.ContactEmail,

                    "New Appointment Booked",

                    $"Patient {patientUser.Username} booked an appointment on {slot.Date_Time:dd MMM yyyy hh:mm tt}.\nReason: {slot.Reason ?? "Not specified"}"

                )

            );

            // Email to Patient

            var formattedDate = slot.Date_Time.ToString("dd MMMM yyyy 'at' hh:mm tt");

            var patientEmailBody = $@"

            Dear {patientUser.Username},
 
            Your appointment has been successfully booked.
 
            Date & Time: {formattedDate}

            Doctor: {doctor?.Name ?? "Assigned Doctor"}

            Reason: {dto.Reason}

            Fee: ₹{slot.AppointmentFee}
 
            Thank you for choosing IntelliCare.";

            _backgroundJobs.Enqueue(() =>

                _emailSender.SendEmailAsync(

                    patientUser.ContactEmail,

                    "Appointment Confirmation",

                    patientEmailBody

                )

            );

            var doctorMobile = PhoneFormatter.FormatToE164(doctorUser.MobileNumber);

            var patientMobile = PhoneFormatter.FormatToE164(patientUser.MobileNumber);

            // WhatsApp to Doctor

            var doctorWhatsAppBody = $"🩺 IntelliCare Alert:\nPatient {patientUser.Username} booked an appointment on {slot.Date_Time:dd MMM yyyy hh:mm tt}.\nReason: {slot.Reason ?? "Not specified"}";

            _backgroundJobs.Enqueue(() =>

                _whatsappSender.SendMessageAsync(doctorMobile, doctorWhatsAppBody)

            );

            // WhatsApp to Patient

            var patientWhatsAppBody = $"✅ Appointment Confirmed:\n {doctor?.Name ?? "Assigned Doctor"}\nDate & Time: {formattedDate}\nFee: ₹{slot.AppointmentFee}\nThank you for choosing IntelliCare.";

            _backgroundJobs.Enqueue(() =>

                _whatsappSender.SendMessageAsync(patientMobile, patientWhatsAppBody)

            );

            // 🕒 Schedule WhatsApp reminder 5 minutes before appointment

            var reminderTime = slot.Date_Time.AddMinutes(-5);

            BackgroundJob.Schedule(() =>

                SendPreAppointmentReminderAsync(slot.AppointmentID),

                reminderTime);

            await _notifier.NotifyQueueUpdateAsync(dto.DoctorID);

            return new BookAppointmentResult

            {

                Message = "Appointment booked successfully.",

                Fee = slot.AppointmentFee,

                PaymentRequired = true,

                AppointmentID = slot.AppointmentID

            };

        }



        public async Task SendPreAppointmentReminderAsync(int appointmentId)
        {
            var appt = await _repo.GetByIdAsync(appointmentId);
            if (appt == null || appt.Status != AppointmentStatus.Booked || appt.PatientID == null)
                return;

            var user = await _repo.GetUserByPatientIdAsync(appt.PatientID.Value);
            if (user == null) return;

            var mobile = PhoneFormatter.FormatToE164(user.MobileNumber);
            var time = appt.Date_Time.ToString("hh:mm tt");
            var message = $"👋 You're next! Your appointment is at {time}. Please be ready.";

            await _whatsappSender.SendMessageAsync(mobile, message);
            _logger.LogInformation("WhatsApp reminder sent to {Mobile} for appointment ID {AppointmentID}", mobile, appointmentId);
        }

        public async Task<List<AppointmentDto>> GetBookedAppointmentsForDoctorByDateAsync(string doctorUsername, DateTime date)
        {
            var doctorUser = await _repo.GetUserByUsernameAsync(doctorUsername);
            if (doctorUser == null || doctorUser.DoctorID == null)
                throw new InvalidOperationException("Doctor not found or profile incomplete.");

            var appointments = await _repo.GetBookedAppointmentsForDoctorByDateAsync(doctorUser.DoctorID.Value, date);

            return appointments.Select(a => new AppointmentDto
            {
                AppointmentId = a.AppointmentID,
                ScheduledTime = a.Date_Time,
                Status = a.Status.ToString(),
                PatientName = a.Patient?.FullName ?? a.Patient?.UserName ?? "Unknown",
                Reason = a.Reason
            }).ToList();
        }





        public async Task AutoCompleteExpiredAppointmentsAsync()
        {
            var now = DateTime.Now;
            var expiredAppointments = await _repo.GetAppointmentsEndingBeforeAsync(now);

            foreach (var appt in expiredAppointments)
            {
                if (appt.Status == AppointmentStatus.Booked || appt.Status == AppointmentStatus.InProgress)
                {
                    appt.Status = AppointmentStatus.Completed;
                    await _repo.UpdateAsync(appt);

                    await RecalculateQueueAsync(appt.DoctorID, appt.Date_Time.Date);

                    var nextAppointment = await _repo.GetNextBookedAppointmentAsync(
                        appt.DoctorID,
                        appt.Date_Time.Date,
                        appt.Date_Time);

                    if (nextAppointment?.PatientID != null)
                    {
                        var patientUser = await _repo.GetUserByPatientIdAsync(nextAppointment.PatientID.Value);
                        var mobile = PhoneFormatter.FormatToE164(patientUser.MobileNumber);
                        var time = nextAppointment.Date_Time.ToString("hh:mm tt");

                        var message = $"👋 You're next! Your appointment is at {time}. Please be ready.";
                        _backgroundJobs.Enqueue(() => _whatsappSender.SendMessageAsync(mobile, message));
                    }

                    await _notifier.NotifyQueueUpdateAsync(appt.DoctorID);
                }
            }
        }

        public async Task AutoUpdateInProgressAppointmentsAsync()
        {
            var now = DateTime.Now;

            // Get appointments that should now be in progress
            var appointments = await _repo.GetAppointmentsStartingBeforeAsync(now);

            foreach (var appt in appointments)
            {
                if (appt.Status == AppointmentStatus.Booked)
                {
                    appt.Status = AppointmentStatus.InProgress;
                    await _repo.UpdateAsync(appt);

                    _logger.LogInformation("Appointment ID {AppointmentId} marked as InProgress", appt.AppointmentID);
                    await _notifier.NotifyQueueUpdateAsync(appt.DoctorID);
                }
            }
        }


        public async Task<List<AppointmentDto>> GetAppointmentsForDoctorAsync(string doctorUsername)
        {
            var doctorUser = await _repo.GetUserByUsernameAsync(doctorUsername);
            if (doctorUser == null || doctorUser.DoctorID == null)
                throw new InvalidOperationException("Doctor not found or profile incomplete.");

            // Fetch only booked appointments for today
            var appointments = await _repo.GetBookedAppointmentsForDoctorByDateAsync(doctorUser.DoctorID.Value, DateTime.Today);

            return appointments.Select(a => new AppointmentDto
            {
                AppointmentId = a.AppointmentID,
                ScheduledTime = a.Date_Time,
                Status = a.Status.ToString(),
                PatientName = a.Patient?.FullName ?? "Unknown",
                Reason = a.Reason
            }).ToList();
        }



        public async Task<PatientSummaryDto> GetPatientSummaryByAppointmentIdAsync(int appointmentId)
        {
            var appointment = await _repo.GetAppointmentByIdAsync(appointmentId);

            if (appointment == null)
                throw new ArgumentException("Appointment not found.");

            if (appointment.Patient == null)
                throw new InvalidOperationException("No patient is assigned to this appointment.");

            var p = appointment.Patient;

            return new PatientSummaryDto
            {
                PatientId = p.PatientId,
                FullName = p.FullName,
                PhoneNumber = p.PhoneNumber,
                Gender = p.Gender.ToString(),
                BloodGroup = p.BloodGroup,
                MedicalHistory = p.MedicalHistory,
                Age = p.Age // ✅ Already stored in entity
            };
        }

        private async Task RecalculateQueueAsync(int doctorId, DateTime date)
        {
            var appointments = await _repo.GetBookedAppointmentsForDoctorByDateAsync(doctorId, date);

            int position = 1;
            foreach (var appt in appointments.OrderBy(a => a.Date_Time))
            {
                appt.QueuePosition = position++;
                await _repo.UpdateAsync(appt);
            }
        }

        public async Task MarkAppointmentCompletedAsync(int appointmentId)
        {
            var appointment = await _repo.GetAppointmentByIdAsync(appointmentId);
            if (appointment == null)
                throw new InvalidOperationException("Appointment not found.");

            if (appointment.Status != AppointmentStatus.Booked && appointment.Status != AppointmentStatus.InProgress)
                throw new InvalidOperationException("Only booked or in-progress appointments can be marked as completed.");

            appointment.Status = AppointmentStatus.Completed;
            await _repo.UpdateAsync(appointment);

            // Recalculate queue for remaining appointments on the same day
            await RecalculateQueueAsync(appointment.DoctorID, appointment.Date_Time.Date);

            // Notify next patient in line
            var nextAppointment = await _repo.GetNextBookedAppointmentAsync(
                appointment.DoctorID,
                appointment.Date_Time.Date,
                appointment.Date_Time);

            if (nextAppointment?.PatientID != null)
            {
                var patientUser = await _repo.GetUserByPatientIdAsync(nextAppointment.PatientID.Value);
                var mobile = PhoneFormatter.FormatToE164(patientUser.MobileNumber);
                var time = nextAppointment.Date_Time.ToString("hh:mm tt");

                var message = $"👋 You're next! Your appointment is at {time}. Please be ready.";
                _backgroundJobs.Enqueue(() => _whatsappSender.SendMessageAsync(mobile, message));
            }

            await _notifier.NotifyQueueUpdateAsync(appointment.DoctorID);
        }


        public async Task RescheduleAsync(RescheduleAppointmentDto dto)
        {
            var patientId = _userService.GetLoggedInPatientId();
            var oldSlot = await _repo.GetAppointmentByIdAsync(dto.AppointmentId);
            var patientUser = await _repo.GetUserByPatientIdAsync(patientId);

            if (oldSlot == null || oldSlot.PatientID != patientId)
                throw new UnauthorizedAccessException("You can only reschedule your own appointments.");

            // ⛔ Restrict rescheduling for past or completed appointments
            if (oldSlot.Date_Time < DateTime.Now)
                throw new InvalidOperationException("You cannot reschedule a past appointment.");

            if (oldSlot.Status == AppointmentStatus.Completed || oldSlot.Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("This appointment is already completed or cancelled and cannot be rescheduled.");

            // ✅ Validate new slot
            var newSlot = await _repo.GetAvailableSlotAsync(oldSlot.DoctorID, dto.NewDate_Time);
            if (newSlot == null || newSlot.PatientID != null)
                throw new InvalidOperationException("New slot is not available.");

            var existingAppointments = await _repo.GetAppointmentsInBlockAsync(oldSlot.DoctorID, dto.NewDate_Time);
            int queuePosition = existingAppointments.Count + 1;

            // Cancel old slot
            oldSlot.PatientID = null;
            oldSlot.Status = AppointmentStatus.ReopenedFromReschedule;
            oldSlot.QueuePosition = null;
            oldSlot.Reason = string.Empty;


            // Book new slot
            newSlot.PatientID = patientId;
            newSlot.Status = AppointmentStatus.Booked;
            newSlot.QueuePosition = queuePosition;
            newSlot.Reason = oldSlot.Reason;

            await _repo.UpdateAsync(oldSlot);
            await _repo.UpdateAsync(newSlot);

            var doctorUser = await _repo.GetUserByDoctorIdAsync(oldSlot.DoctorID);
            var doctor = await _repo.GetDoctorByIdAsync(oldSlot.DoctorID);

            var formattedDate = newSlot.Date_Time.ToString("dd MMMM yyyy 'at' hh:mm tt");
            var previousDate = oldSlot.Date_Time.ToString("dd MMMM yyyy 'at' hh:mm tt");

            var doctorMobile = PhoneFormatter.FormatToE164(doctorUser.MobileNumber);
            var patientMobile = PhoneFormatter.FormatToE164(patientUser.MobileNumber);

            // WhatsApp to Doctor
            var doctorMessage = $"🔄 Appointment Rescheduled:\nPatient {patientUser.Username} moved their appointment to {formattedDate}.\nPrevious slot was {previousDate}.";
            _backgroundJobs.Enqueue(() =>
                _whatsappSender.SendMessageAsync(doctorMobile, doctorMessage)
            );

            // WhatsApp to Patient
            var patientMessage = $"✅ Your appointment has been rescheduled.\nDr. {doctor?.Name ?? "Assigned Doctor"}\nNew Date & Time: {formattedDate}\nPrevious: {previousDate}\nThank you for choosing IntelliCare.";
            _backgroundJobs.Enqueue(() =>
                _whatsappSender.SendMessageAsync(patientMobile, patientMessage)
            );

            // Email to Doctor
            _backgroundJobs.Enqueue(() =>
                _emailSender.SendEmailAsync(
                    doctorUser.ContactEmail,
                    "Appointment Rescheduled",
                    doctorMessage
                )
            );

            // Email to Patient
            _backgroundJobs.Enqueue(() =>
                _emailSender.SendEmailAsync(
                    patientUser.ContactEmail,
                    "Appointment Rescheduled",
                    patientMessage
                )
            );

            await _notifier.NotifyQueueUpdateAsync(oldSlot.DoctorID);
        }


        public async Task<string> CreateSlotsAsync(CreateSlotDto dto)

        {

            var roleClaim = _httpContextAccessor.HttpContext?.User?.Claims

                .FirstOrDefault(c => c.Type.Contains("role"));

            if (roleClaim == null || roleClaim.Value != "Admin")

                throw new UnauthorizedAccessException("Only admins can create slots.");

            if (dto.DoctorID <= 0)

                throw new ArgumentException("DoctorID must be a valid non-zero value.");

            if (!await _repo.DoctorExistsAsync(dto.DoctorID))

                throw new ArgumentException($"Doctor with ID {dto.DoctorID} does not exist.");


            if (!TimeSpan.TryParse(dto.StartTime, out var startTimeSpan) ||

                !TimeSpan.TryParse(dto.EndTime, out var endTimeSpan))

                throw new ArgumentException("Invalid time format. Use HH:mm:ss.");

            var start = dto.Date.Date.Add(startTimeSpan);

            var end = dto.Date.Date.Add(endTimeSpan);

            var now = DateTime.Now;

            if (start >= end)

                throw new ArgumentException("Start time must be before end time.");

            if (end <= now)

                throw new ArgumentException("Cannot create slots entirely in the past.");

            var proposedSlots = new List<DateTime>();

            var current = start;

            while (current < end)

            {

                if (current > now)

                    proposedSlots.Add(current);

                current = current.AddMinutes(dto.IntervalMinutes);

            }

            if (!proposedSlots.Any())

                return "⚠️ No valid future slots to create.";

            var finalSlots = new List<Appointment>();

            foreach (var slotTime in proposedSlots)

            {

                var exists = await _repo.SlotExistsAsync(dto.DoctorID, slotTime);

                if (!exists)

                {

                    finalSlots.Add(new Appointment

                    {

                        DoctorID = dto.DoctorID,

                        Date_Time = slotTime,

                        SlotDurationMinutes = dto.IntervalMinutes,

                        Status = AppointmentStatus.Available,

                        AppointmentFee = dto.Fee

                    });

                }

            }

            if (!finalSlots.Any())

                return "ℹ All proposed slots already exist.";

            await _repo.AddRangeAsync(finalSlots);

            return "✅ Doctor slots created successfully.";

        }






        public async Task CancelAsync(CancelAppointmentDto dto)
        {
            var patientId = _userService.GetLoggedInPatientId();
            var slot = await _repo.GetAppointmentByIdAsync(dto.AppointmentID);
            var patientUser = await _repo.GetUserByPatientIdAsync(patientId);

            if (slot == null || slot.PatientID != patientId)
                throw new UnauthorizedAccessException("You can only cancel your own appointments.");

            // ⛔ Prevent cancellation of completed or expired appointments
            if (slot.Status == AppointmentStatus.Completed || slot.Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("This appointment is already completed or cancelled.");

            if (slot.Date_Time < DateTime.Now)
                throw new InvalidOperationException("You cannot cancel a past appointment.");

            _logger.LogInformation("Cancelling appointment ID {AppointmentID} for patient ID {PatientID}", dto.AppointmentID, patientId);

            // ✅ Cancel slot
            slot.PatientID = null;
            slot.Status = AppointmentStatus.ReopenedFromCancellation;
            slot.QueuePosition = null;
            slot.Reason = string.Empty;

            await _repo.UpdateAsync(slot);

            var doctorUser = await _repo.GetUserByDoctorIdAsync(slot.DoctorID);
            var doctor = await _repo.GetDoctorByIdAsync(slot.DoctorID);

            var formattedDate = slot.Date_Time.ToString("dd MMMM yyyy 'at' hh:mm tt");

            var doctorMobile = PhoneFormatter.FormatToE164(doctorUser.MobileNumber);
            var patientMobile = PhoneFormatter.FormatToE164(patientUser.MobileNumber);

            // 📲 WhatsApp to Doctor
            var doctorMessage = $"❌ Appointment Cancelled:\nPatient {patientUser.Username} cancelled their appointment scheduled for {formattedDate}.";
            _backgroundJobs.Enqueue(() =>
                _whatsappSender.SendMessageAsync(doctorMobile, doctorMessage)
            );

            // 📲 WhatsApp to Patient
            var patientMessage = $"🗓️ Your appointment with Dr. {doctor?.Name ?? "Assigned Doctor"} on {formattedDate} has been cancelled.\nWe hope to see you again soon.";
            _backgroundJobs.Enqueue(() =>
                _whatsappSender.SendMessageAsync(patientMobile, patientMessage)
            );

            // 📧 Email to Doctor
            _backgroundJobs.Enqueue(() =>
                _emailSender.SendEmailAsync(
                    doctorUser.ContactEmail,
                    "Appointment Cancelled",
                    doctorMessage
                )
            );

            // 📧 Email to Patient
            _backgroundJobs.Enqueue(() =>
                _emailSender.SendEmailAsync(
                    patientUser.ContactEmail,
                    "Appointment Cancelled",
                    patientMessage
                )
            );

            // 🔄 Notify queue update
            await _notifier.NotifyQueueUpdateAsync(slot.DoctorID);

            _logger.LogInformation("Appointment ID {AppointmentID} cancelled successfully", dto.AppointmentID);
        }


        public async Task CompleteAppointmentAsync(int appointmentId)
        {
            var slot = await _repo.GetAppointmentByIdAsync(appointmentId);
            if (slot == null)
                throw new InvalidOperationException("Appointment not found.");

            if (slot.Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("This appointment is already marked as completed.");

            if (slot.Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Cancelled appointments cannot be completed.");

            _logger.LogInformation("Completing appointment ID {AppointmentID} for doctor ID {DoctorID}", appointmentId, slot.DoctorID);

            // ✅ Mark current appointment as completed
            slot.Status = AppointmentStatus.Completed;
            await _repo.UpdateAsync(slot);

            // 🔄 Recalculate queue and notify next patient
            var next = await _repo.GetNextPatientInQueueAsync(slot.DoctorID, slot.Date_Time);
            if (next != null && next.PatientID.HasValue)
            {
                var patientUser = await _repo.GetUserByPatientIdAsync(next.PatientID.Value);
                var doctor = await _repo.GetDoctorByIdAsync(slot.DoctorID);

                var formattedTime = next.Date_Time.ToString("hh:mm tt");
                var doctorName = doctor?.Name ?? "Assigned Doctor";

                var patientMobile = PhoneFormatter.FormatToE164(patientUser.MobileNumber);

                // 📲 WhatsApp message
                var whatsappMessage = $"⏳ You're Next!\nDr. {doctorName} will see you at {formattedTime}.\nPlease be prepared.";
                _backgroundJobs.Enqueue(() =>
                    _whatsappSender.SendMessageAsync(patientMobile, whatsappMessage)
                );

                // 📧 Email message
                var emailMessage = $"Dear {patientUser.Username},\n\nYou're next in queue for your appointment with Dr. {doctorName} at {formattedTime}.\nPlease be prepared and arrive on time.\n\nThank you,\nIntelliCare Team";
                _backgroundJobs.Enqueue(() =>
                    _emailSender.SendEmailAsync(
                        patientUser.ContactEmail,
                        "You're Next!",
                        emailMessage
                    )
                );

                _logger.LogInformation("Next patient notified: {PatientID} for doctor ID {DoctorID}", next.PatientID, slot.DoctorID);
            }
            else
            {
                _logger.LogInformation("No next patient found in queue for doctor ID {DoctorID}", slot.DoctorID);
            }

            await _notifier.NotifyQueueUpdateAsync(slot.DoctorID);
        }



        public async Task<List<DoctorAvailabilityDto>> GetDoctorAvailabilityAsync(int doctorId)

        {

            var appointments = await _repo.GetFutureAvailableAppointmentsAsync(doctorId);

            return appointments.Select(a => new DoctorAvailabilityDto

            {

                SlotID = a.AppointmentID,

                DoctorID = a.DoctorID,

                DoctorName = a.Doctor?.Name ?? "Unknown",

                Date_Time = a.Date_Time,

                Status = a.Status.ToString(),

                AppointmentFee = a.AppointmentFee

            }).ToList();

        }



        public async Task<List<PatientQueueDto>> GetQueueAsync(int doctorId, DateTime date)
        {
            var appointments = await _repo.GetDoctorQueueByDateAsync(doctorId, date);

            return appointments.Select(a => new PatientQueueDto
            {
                DoctorID = a.DoctorID,
                PatientID = a.PatientID ?? 0,
                Date_Time = a.Date_Time,
                SlotDurationMinutes = a.SlotDurationMinutes,
                QueuePosition = a.QueuePosition ?? 0
            }).ToList();
        }



        public async Task<List<AppointmentDto>> GetAllAppointmentsForTodayAsync()
        {
            var appointments = await _repo.GetAllBookedAppointmentsByDateAsync(DateTime.Today);

            return appointments.Select(a => new AppointmentDto
            {
                AppointmentId = a.AppointmentID,
                ScheduledTime = a.Date_Time,
                Status = a.Status.ToString(),
                PatientName = a.Patient?.FullName ?? "Unknown",
                Reason = a.Reason,
                DoctorName = a.Doctor?.Name ?? "Unknown"
            }).OrderByDescending(a => a.ScheduledTime).ToList();
        }



        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            var appointments = await _repo.GetAllAppointmentsAsync();

            return appointments.Select(a => new AppointmentDto
            {
                AppointmentId = a.AppointmentID,
                ScheduledTime = a.Date_Time,
                Status = a.Status.ToString(), // Convert enum to string
                PatientName = a.Patient?.FullName ?? "N/A",
                DoctorName = a.Doctor?.Name ?? "N/A",
                Reason = a.Reason,
                AmountPaid = a.AppointmentFee
            });
        }





        public async Task<List<MyAppointmentDto>> GetAppointmentsByPatientIdAsync(int patientId)

        {

            // Fetch appointments from repository

            var appointments = await _repo.GetAppointmentsByPatientIdAsync(patientId);

            // debug-log the raw enum values coming from repository

            _logger.LogDebug("Appointments for patient {PatientId}: {Statuses}",

                patientId, string.Join(", ", appointments.Select(a => a.Status.ToString())));

            var result = appointments.Select(a => new MyAppointmentDto

            {

                AppointmentID = a.AppointmentID,

                DoctorID = a.DoctorID,

                DoctorName = a.Doctor?.Name,

                DoctorPhotoUrl = a.Doctor?.PhotoUrl,

                DoctorSpecialization = a.Doctor?.Specialization,

                DoctorAddress = a.Doctor?.Address,

                Date_Time = a.Date_Time.ToString("o"), // ISO 8601

                Status = a.Status.ToString(),           // <- ensure enum -> string

                AppointmentFee = a.AppointmentFee,

                SlotDurationMinutes = a.SlotDurationMinutes,

                QueuePosition = a.QueuePosition

            }).ToList();

            return result;

        }

    }
}

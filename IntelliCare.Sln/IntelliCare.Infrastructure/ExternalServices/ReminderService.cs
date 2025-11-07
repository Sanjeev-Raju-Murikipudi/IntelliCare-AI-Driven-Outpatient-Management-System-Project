using IntelliCare.Application.Interfaces;
using IntelliCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

public class ReminderService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notifier;

    public ReminderService(ApplicationDbContext context, INotificationService notifier)
    {
        _context = context;
        _notifier = notifier;
    }

    public async Task SendRemindersAsync()
    {
        var targetDate = DateTime.Now.AddDays(1).Date;

        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Where(a => a.Date_Time.Date == targetDate && a.Status == AppointmentStatus.Booked)
            .ToListAsync();

        foreach (var appt in appointments)
        {
            if (appt.Patient == null || string.IsNullOrEmpty(appt.Patient.UserName))
                continue;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == appt.Patient.UserName && u.RoleName == UserRole.Patient);

            if (user == null || string.IsNullOrEmpty(user.ContactEmail))
                continue;

            await _notifier.SendEmailAsync(
                user.ContactEmail,
                "Appointment Reminder",
                $"Dear {appt.Patient.FullName}, your appointment with Dr. {appt.DoctorID} is scheduled for {appt.Date_Time:dd MMM yyyy hh:mm tt}.");
        }
    }
}

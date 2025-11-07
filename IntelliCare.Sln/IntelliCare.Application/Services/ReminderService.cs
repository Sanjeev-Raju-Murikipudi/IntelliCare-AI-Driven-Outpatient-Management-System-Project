//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using IntelliCare.Application.Services;

//public class ReminderService : BackgroundService
//{
//    private readonly IServiceScopeFactory _scopeFactory;

//    public ReminderService(IServiceScopeFactory scopeFactory)
//    {
//        _scopeFactory = scopeFactory;
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            using var scope = _scopeFactory.CreateScope();
//            var appointmentService = scope.ServiceProvider.GetRequiredService<AppointmentService>();

//            var targetTime = DateTime.UtcNow.AddHours(24);
//            var appointments = await appointmentService.GetRemindersAsync(targetTime);

//            foreach (var appt in appointments)
//            {
//                Console.WriteLine($"Reminder: Appointment at {appt.Date_Time} for Patient {appt.PatientID}");
//            }

//            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
//        }
//    }
//}
//A
using System.Text;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.SqlServer;
using IntelliCare.API.Hubs;
using IntelliCare.API.Notifications;
using IntelliCare.Application.Configurations;
using IntelliCare.Application.Interfaces;
using IntelliCare.Application.Mappings;
using IntelliCare.Application.Services;
using IntelliCare.Infrastructure;
using IntelliCare.Infrastructure.ExternalServices;
using IntelliCare.Infrastructure.Persistence.Repositories;
using IntelliCare.Infrastructure.Repositories;
using IntelliCare.Infrastructure.Services;
using IntelliCare.WebAPI.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Text.Json.Serialization;
using Serilog; // ?? Serilog


System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

// ?? Serilog: Configure logger before builder
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/intellicare-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

QuestPDF.Settings.License = LicenseType.Community;

// ?? Serilog: Use Serilog for logging
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection");

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("IntelliCare.Infrastructure");
    });

    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});

// Dependency Injection
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<ISupportDataService, SupportDataService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddHttpClient<ISmsService, SmsService>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<ISupportDataRepository, SupportDataRepository>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();
builder.Services.AddScoped<IClinicalRecordRepository, ClinicalRecordRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<ReminderService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IQueueNotifier, QueueNotifier>();
builder.Services.AddScoped<IPdfGeneratorService, QuestPdfGeneratorService>();
builder.Services.AddScoped<IWhatsAppSender, TwilioWhatsAppSender>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// JSON Enum Serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// JWT Authentication
var jwtSettings = configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                message = "Authentication required. Please log in."
            });
            return context.Response.WriteAsync(result);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                message = "Access Denied. You are not authorized to perform this action."
            });
            return context.Response.WriteAsync(result);
        }
    };
});

builder.Services.AddAuthorization();

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("LoginPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.AddPolicy("ResetPasswordPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(5),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

// Hangfire Configuration
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

// Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// SignalR
builder.Services.AddSignalR();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IntelliCare API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

//frontend----------------------------

builder.Services.AddCors(options =>

{

    options.AddPolicy("AllowAngularDevClient", policy =>

    {

        policy.WithOrigins("http://localhost:4200", "https://localhost:4200") // ✅ Angular dev server

              .AllowAnyHeader()

              .AllowAnyMethod()

              .AllowCredentials(); // Optional if using cookies/auth

    });

});





// Build App
var app = builder.Build();

// Seed Admin + Schedule Hangfire Job
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAdminAsync();

    var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // ?? Hourly reminders to patients
    recurringJobs.AddOrUpdate<ReminderService>(
        "daily-appointment-reminders",
        service => service.SendRemindersAsync(),
        Cron.Hourly);

    // ? Auto-complete expired appointments every 5 minutes
    recurringJobs.AddOrUpdate<AppointmentService>(
        "auto-complete-expired-appointments",
        service => service.AutoCompleteExpiredAppointmentsAsync(),
        Cron.MinuteInterval(5));

    recurringJobs.AddOrUpdate<AppointmentService>(
    "auto-update-inprogress-appointments",
    service => service.AutoUpdateInProgressAppointmentsAsync(),
    Cron.MinuteInterval(2));
}


// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// SecureStorage folder
var storagePath = Path.Combine(Directory.GetCurrentDirectory(), "SecureStorage");
if (!Directory.Exists(storagePath))
{
    Directory.CreateDirectory(storagePath);
}

app.UseHttpsRedirection();



app.UseRouting();
app.UseStaticFiles();


//frontend----------------------------
app.UseCors("AllowAngularDevClient");
app.UseHangfireDashboard("/hangfire");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHub<QueueHub>("/queueHub");

app.Run();

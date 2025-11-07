using IntelliCare.Domain;
using IntelliCare.Domain.Enums;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<ClinicalRecord> ClinicalRecords { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<SupportData> SupportData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Map User to table named "User"
        modelBuilder.Entity<User>().ToTable("User");

        // Enum-to-int conversion for UserRole
        modelBuilder.Entity<User>()
            .Property(u => u.RoleName)
            .HasConversion<int>();

        // ✅ Enum-to-int conversion for AppointmentStatus
        modelBuilder.Entity<Appointment>()
            .Property(a => a.Status)
            .HasConversion<int>();

        // Configure SupportData relationship
        modelBuilder.Entity<SupportData>()
            .HasOne(s => s.Patient)
            .WithMany()
            .HasForeignKey(s => s.DoctorID);

        modelBuilder.Entity<SupportData>()
            .HasKey(s => s.DataID);

        // Map Patient to table named "Patients"
        modelBuilder.Entity<Patient>().ToTable("Patients");

        // Unique index for appointment slots
        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.DoctorID, a.Date_Time })
            .IsUnique();

        // Precision for AppointmentFee
        modelBuilder.Entity<Appointment>()
            .Property(a => a.AppointmentFee)
            .HasPrecision(10, 2);

        modelBuilder
            .Entity<Patient>()
            .Property(p => p.Gender)
            .HasConversion<string>();


        base.OnModelCreating(modelBuilder);
    }
}

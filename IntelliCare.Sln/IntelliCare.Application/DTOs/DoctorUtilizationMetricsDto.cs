namespace IntelliCare.Application.DTOs
{
    public class DoctorUtilizationMetricsDto
    {
        public double UtilizationRate { get; set; } // Percentage of time booked
        public double NoShowRate { get; set; }      // Percentage of no-shows
    }
}
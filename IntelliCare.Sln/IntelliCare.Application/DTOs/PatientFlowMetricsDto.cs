namespace IntelliCare.Application.DTOs
{
    public class PatientFlowMetricsDto
    {
        public double AverageWaitTimeMinutes { get; set; }
        public int PeakFlowHour { get; set; }
    }
}
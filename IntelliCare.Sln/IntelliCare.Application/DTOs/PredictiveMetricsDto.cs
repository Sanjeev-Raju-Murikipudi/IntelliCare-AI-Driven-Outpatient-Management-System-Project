namespace IntelliCare.Application.DTOs
{
    public class PredictiveMetricsDto
    {
        public int ProjectedVisits { get; set; } // Projected number of appointments for the next period
        public double ResourceNeed { get; set; } // Estimated number of new doctors needed
    }
}
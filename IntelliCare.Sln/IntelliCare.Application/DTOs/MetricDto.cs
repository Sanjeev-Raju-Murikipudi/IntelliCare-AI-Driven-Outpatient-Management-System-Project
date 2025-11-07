// IntelliCare.Application.DTOs/MetricDto.cs

namespace IntelliCare.Application.DTOs
{
    public class MetricDto
    {
        // FIX: Initialize string properties
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
    }
}
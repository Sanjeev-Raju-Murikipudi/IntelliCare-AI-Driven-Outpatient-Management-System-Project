// IntelliCare.Application.DTOs/ReportSummaryDto.cs

using System;
using System.Collections.Generic;

namespace IntelliCare.Application.DTOs
{
    public class ReportSummaryDto
    {
        public int ReportID { get; set; }

        public string Type { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }

        public List<MetricDto> Metrics { get; set; } = new List<MetricDto>();
        public object? DetailedData { get; set; }
    }
}
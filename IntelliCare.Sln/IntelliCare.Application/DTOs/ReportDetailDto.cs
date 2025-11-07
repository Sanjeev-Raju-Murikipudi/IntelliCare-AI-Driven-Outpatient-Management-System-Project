// IntelliCare.Application.DTOs/ReportDetailDto.cs

using System;

namespace IntelliCare.Application.DTOs
{
    public class ReportDetailDto
    {
        public int ReportID { get; set; }

        // FIX: Initialize string properties
        public string Type { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }

        // These properties hold the raw JSON data saved in the database columns.
        public string MetricsJson { get; set; } = string.Empty;
        public string DetailedDataJson { get; set; } = string.Empty;
    }
}
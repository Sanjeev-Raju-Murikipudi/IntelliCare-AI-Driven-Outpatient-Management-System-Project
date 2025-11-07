// IntelliCare.Application.DTOs/ReportRequestDto.cs

using System;
using System.ComponentModel.DataAnnotations;

namespace IntelliCare.Application.DTOs
{
    public class ReportRequestDto
    {
        [Required]
        public string ReportType { get; set; } = string.Empty; // FIX: Initialize string

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int? DoctorId { get; set; }
    }
}
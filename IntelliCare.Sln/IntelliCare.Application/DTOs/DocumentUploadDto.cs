using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace IntelliCare.Application.DTOs
{
    public class DocumentUploadDto
    {
        [Required(ErrorMessage = "File is required.")]
        public IFormFile File { get; set; }

        [Required(ErrorMessage = "PatientId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "PatientId must be a positive number.")]
        public int PatientId { get; set; }
    }
}

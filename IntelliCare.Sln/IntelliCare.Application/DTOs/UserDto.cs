using System.ComponentModel.DataAnnotations;
using IntelliCare.Domain.Enums;

namespace IntelliCare.Application.Models
{
    public class UserDto
    {
        public int UserID { get; set; }

        public string Username { get; set; }

        public UserRole RoleName { get; set; }

        public string MobileNumber { get; set; }

        //public int? PatientID { get; set; }

        //public int? DoctorID { get; set; }

        //[Required]
        //[StringLength(255)]
        //[EmailAddress]
        //public string ContactEmail { get; set; }

       // public string? APIToken { get; set; } // Optional: used if token is issued
    }
}

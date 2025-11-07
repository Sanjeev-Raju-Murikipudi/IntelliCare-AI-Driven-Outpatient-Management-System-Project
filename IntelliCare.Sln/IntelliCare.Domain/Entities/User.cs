using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IntelliCare.Domain.Enums;

namespace IntelliCare.Domain
{
    // LLD 8.3: Security & RBAC
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        public UserRole RoleName { get; set; }

        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string ContactEmail { get; set; }


        //[StringLength(255)]
        //public string? APIToken { get; set; }



        public int? PatientID { get; set; }
        public int? DoctorID { get; set; }

        [ForeignKey("PatientID")]
        public virtual Patient Patient { get; set; }

        [ForeignKey("DoctorID")]
        public virtual Doctor Doctor { get; set; }

        // ✅ OTP fields for secure login
        [StringLength(10)]
        public string? OTPCode { get; set; }

        public DateTime? OTPExpiry { get; set; }


        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEndTime { get; set; }
        public string? PasswordResetOTP { get; set; }
        public DateTime? PasswordResetExpiry { get; set; }

    }
}

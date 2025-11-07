using IntelliCare.Domain.Enums;
using IntelliCare.Domain.Validators;
using System;
using System.ComponentModel.DataAnnotations;

namespace IntelliCare.Application.DTOs
{
    /// <summary>
    /// DTO for completing a patient's profile after registration.
    /// Used to create the Patient entity and link it to the User based on Username.
    /// </summary>
    public class CreatePatientDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username can't exceed 50 characters.")]
        public string Username { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        [PastDate(ErrorMessage = "Date of birth must be in the past.")]
        public DateTime DOB { get; set; }

        //[Required(ErrorMessage = "Age is required.")]
        //[Range(0, 120, ErrorMessage = "Age must be between 0 and 120.")]
        //public int Age { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender.")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Blood group is required.")]
        [RegularExpression(@"^(A|B|AB|O)[+-]$", ErrorMessage = "Blood group must be A+, A-, B+, B-, AB+, AB-, O+, or O-.")]
        public string BloodGroup { get; set; }

        [StringLength(500, ErrorMessage = "Insurance details can't exceed 500 characters.")]
        public string? InsuranceDetails { get; set; }

        [Required(ErrorMessage = "Medical history is required.")]
        [StringLength(1000, ErrorMessage = "Medical history can't exceed 1000 characters.")]
        public string MedicalHistory { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200)]
        public string Address { get; set; }

    }
}

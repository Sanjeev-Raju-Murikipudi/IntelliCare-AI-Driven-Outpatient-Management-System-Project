using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IntelliCare.Domain.Enums;

namespace IntelliCare.Domain;


[Table("Patients")]
public class Patient
{

    public int PatientId { get; set; }
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name can't exceed 100 characters.")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name can't exceed 100 characters.")]
    public string UserName { get; set; }
    public DateTime DOB { get; set; }

    //[Required]
    //[EmailAddress]
    //public string ContactEmail { get; set; }

    

    //public string Name { get; set; }
    public string InsuranceDetails { get; set; }
    public string MedicalHistory { get; set; }

    [Required(ErrorMessage = "Age is required.")]
    [Range(0, 120, ErrorMessage = "Age must be between 0 and 120.")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Gender is required.")]
    [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender.")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Blood group is required.")]
    [RegularExpression(@"^(A|B|AB|O)[+-]$", ErrorMessage = "Blood group must be A+, A-, B+, B-, AB+, AB-, O+, or O-.")]
    public string BloodGroup { get; set; }

    [Required]
    [RegularExpression(@"^\d{10}$")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Address is required.")]
    [StringLength(200, ErrorMessage = "Address can't exceed 200 characters.")]
    public string Address { get; set; }


    // Navigation property to appointments, to be used later in Module 4.2.
    public ICollection<Appointment> Appointments { get; set; }

    

}
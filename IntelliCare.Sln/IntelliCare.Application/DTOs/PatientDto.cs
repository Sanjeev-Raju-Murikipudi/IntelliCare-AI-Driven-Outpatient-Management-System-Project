using System.ComponentModel.DataAnnotations;

namespace IntelliCare.Application.DTOs

{

    public class PatientDto

    {

        [Required]

        public int PatientId { get; set; }

        [Required(ErrorMessage = "Name is required.")]

        [StringLength(100, ErrorMessage = "Name can't be longer than 100 characters.")]

        public string Name { get; set; }

        [DataType(DataType.PhoneNumber)]

        [Required(ErrorMessage = "Phone number is required.")]

        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits.")]

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must contain only digits.")]

        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Contact Email is required.")]

        [Display(Name = "Contact Email")]

        [EmailAddress(ErrorMessage = "Invalid email format.")]

        public string ContactEmail { get; set; }

        public string FullName { get; set; }

        public DateTime DOB { get; set; }

        public string Gender { get; set; }

        public string BloodGroup { get; set; }

        public int Age { get; set; }

        [Required(ErrorMessage = "Insurance Details are required.")]

        public string InsuranceDetails { get; set; }

        [Required(ErrorMessage = "Medical History is required.")]

        public string MedicalHistory { get; set; }

        public string Address { get; set; }

        public bool IsProfileComplete =>

        !string.IsNullOrWhiteSpace(ContactEmail) &&

        DOB != default &&

        !string.IsNullOrWhiteSpace(PhoneNumber) &&

        !string.IsNullOrWhiteSpace(MedicalHistory);

    }

}

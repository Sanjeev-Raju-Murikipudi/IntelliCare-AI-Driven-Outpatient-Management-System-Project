using System.ComponentModel.DataAnnotations;

namespace IntelliCare.Application.DTOs

{

    public class CompleteDoctorProfileDto

    {

        [Required(ErrorMessage = "Username is required.")]

        [StringLength(50, ErrorMessage = "Username can't be longer than 50 characters.")]

        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]

        public string Username { get; set; }

        [Required(ErrorMessage = "Doctor name is required.")]

        [StringLength(100, ErrorMessage = "Name can't be longer than 100 characters.")]

        public string Name { get; set; }


        [Required(ErrorMessage = "Specialization is required.")]

        [StringLength(100, ErrorMessage = "Specialization can't be longer than 100 characters.")]

        public string Specialization { get; set; }

        [StringLength(250)]

        public string Education { get; set; }

        [StringLength(250)]

        public string Address { get; set; }

        [Range(0, 100, ErrorMessage = "Experience must be between 0 and 100 years.")]

        public int ExperienceYears { get; set; }

        [StringLength(255)]

        public string PhotoUrl { get; set; }

    }

}


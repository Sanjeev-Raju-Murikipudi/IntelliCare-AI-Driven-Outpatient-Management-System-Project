using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace IntelliCare.Domain

{

    // LLD 2.2: Provider Management

    [Table("Doctor")]

    public class Doctor

    {

        [Key]

        public int DoctorId { get; set; }

        [Required]

        public string Name { get; set; }

        [Required]

        public string Specialization { get; set; }



        [StringLength(250)]

        public string Education { get; set; } = "";

        [StringLength(250)]

        public string Address { get; set; } = "";

        [Range(0, 100)]

        public int ExperienceYears { get; set; } = 0;

        [StringLength(255)]

        public string PhotoUrl { get; set; } = "";

        // Optional: Navigation to User

        public ICollection<Appointment> Appointments { get; set; }

    }

}


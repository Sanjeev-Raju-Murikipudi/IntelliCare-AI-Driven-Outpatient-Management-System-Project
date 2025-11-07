using System.ComponentModel.DataAnnotations;

public class CreateDoctorDto
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, ErrorMessage = "Username can't be longer than 50 characters.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits.")]
    public string MobileNumber { get; set; }

    //[Required(ErrorMessage = "Doctor name is required.")]
    //[StringLength(100, ErrorMessage = "Name can't be longer than 100 characters.")]
    //public string Name { get; set; }


    [Required(ErrorMessage = "Doctor creation key is required.")]
    public string DoctorCreationKey { get; set; }
   
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string ContactEmail { get; set; }

}

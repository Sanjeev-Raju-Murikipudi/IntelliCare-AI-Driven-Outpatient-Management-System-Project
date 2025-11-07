using System;
using System.ComponentModel.DataAnnotations;

public class RegisterUserDto
{

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 100 characters.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
       ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits.")]
    public string MobileNumber { get; set; }

    [Required(ErrorMessage = "Contact email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid format like sanjeevraju@gmail.com.")]
    [StringLength(255, ErrorMessage = "Email can't exceed 255 characters.")]
    public string ContactEmail { get; set; }
}
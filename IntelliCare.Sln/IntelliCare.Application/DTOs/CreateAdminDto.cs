using System.ComponentModel.DataAnnotations;

public class CreateAdminDto
{
    [Required]
    public string Username { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }

    [Required]
    [Phone]
    public string MobileNumber { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string AdminCreationKey { get; set; } // ✅ Add get; set;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string ContactEmail { get; set; }

}

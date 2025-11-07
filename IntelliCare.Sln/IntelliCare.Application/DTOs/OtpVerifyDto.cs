using System.ComponentModel.DataAnnotations;

public class OtpVerifyDto
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string OTPCode { get; set; }
}

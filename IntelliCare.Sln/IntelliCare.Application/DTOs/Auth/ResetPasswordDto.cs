namespace IntelliCare.Application.DTOs.Auth
{
    public class ResetPasswordDto
    {
        public string Username { get; set; }
        public string OTP { get; set; }
        public string NewPassword { get; set; }
    }
}

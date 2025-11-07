using IntelliCare.Application.DTOs;
using IntelliCare.Application.Models;
using IntelliCare.Domain;
using System.Threading.Tasks;

public interface IUserService
{
    Task<object> LoginAsync(LoginRequestDto dto);

    Task RegisterAsync(RegisterUserDto dto); // legacy combined registration
    //Task RegisterPatientAsync(RegisterUserDto dto); // new modular registration

    Task<string> CreateAdminAsync(CreateAdminDto dto, string requesterUsername);

    Task CreateDoctorAsync(CreateDoctorDto dto, string requesterUsername);
    Task<object> VerifyOtpAndGenerateTokenAsync(string username, string otpCode);

    Task<ProfileCompletionResult> CompletePatientProfileAsync(CreatePatientDto dto);

    Task<object> CompleteDoctorProfileAsync(CompleteDoctorProfileDto dto);

    //Task RequestPasswordResetAsync(string email);
    //Task ResetPasswordAsync(string username, string otp, string newPassword);
    int GetLoggedInPatientId();

    string GenerateJwtToken(User user);

    Task<User> GetUserByUsernameAsync(string username);

    Task<bool> ValidateCredentialsAsync(string username, string password);
    //Task<string> GenerateOtpAsync(string username);

    //Task<bool> VerifyOtpAsync(string username, string otpCode);
    Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync();
}

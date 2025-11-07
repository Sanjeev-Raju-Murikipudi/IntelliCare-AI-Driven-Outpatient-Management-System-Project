namespace IntelliCare.Application.Interfaces
{
    public interface ISmsService
    {
        Task SendOtpAsync(string mobileNumber, string message);
    }
}

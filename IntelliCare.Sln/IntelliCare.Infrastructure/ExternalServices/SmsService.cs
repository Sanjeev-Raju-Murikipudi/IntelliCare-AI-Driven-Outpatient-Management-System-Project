using IntelliCare.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class SmsService : ISmsService
{
    private readonly HttpClient _http;
    private readonly ILogger<SmsService> _logger;
    private readonly IConfiguration _config;

    public SmsService(HttpClient http, ILogger<SmsService> logger, IConfiguration config)
    {
        _http = http;
        _logger = logger;
        _config = config;
    }

    public async Task SendOtpAsync(string mobileNumber, string message)
    {
        var apiKey = _config["TextLocal:ApiKey"];
        var sender = _config["TextLocal:Sender"];

        var values = new Dictionary<string, string>
        {
            { "apikey", apiKey },
            { "numbers", mobileNumber },
            { "message", message },
            { "sender", sender }
        };

        var content = new FormUrlEncodedContent(values);

        try
        {
            var response = await _http.PostAsync("https://api.textlocal.in/send/", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("✅ SMS sent successfully: {Response}", responseBody);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ SMS sending failed due to HTTP error");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ SMS sending failed due to unexpected error");
            throw;
        }
    }
}
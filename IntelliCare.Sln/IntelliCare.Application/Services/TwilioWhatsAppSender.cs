using IntelliCare.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class TwilioWhatsAppSender : IWhatsAppSender
{
    private readonly IConfiguration _config;

    public TwilioWhatsAppSender(IConfiguration config)
    {
        _config = config;
        TwilioClient.Init(_config["Twilio:AccountSid"], _config["Twilio:AuthToken"]);
    }

    public async Task SendMessageAsync(string toMobileNumber, string message)
    {
        var from = new PhoneNumber(_config["Twilio:SandboxNumber"]);
        var to = new PhoneNumber($"whatsapp:{toMobileNumber}");

        var msg = await MessageResource.CreateAsync(
            body: message,
            from: from,
            to: to
        );
    }
}

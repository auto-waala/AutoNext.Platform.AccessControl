using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace AutoNext.Platform.AccessControl.API.Managers.Services
{
    public class TwilioSmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TwilioSmsService> _logger;

        public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Initialize Twilio client
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            TwilioClient.Init(accountSid, authToken);
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                var fromNumber = _configuration["Twilio:FromNumber"];

                var result = await MessageResource.CreateAsync(
                    body: message,
                    from: new Twilio.Types.PhoneNumber(fromNumber),
                    to: new Twilio.Types.PhoneNumber(phoneNumber)
                );

                _logger.LogInformation("SMS sent successfully to: {PhoneNumber}, SID: {Sid}", phoneNumber, result.Sid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to: {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        public async Task SendVerificationSmsAsync(string phoneNumber, string otpCode)
        {
            var message = $"Your AutoNext verification code is: {otpCode}. Valid for 10 minutes.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendTwoFactorCodeSmsAsync(string phoneNumber, string code)
        {
            var message = $"Your AutoNext two-factor authentication code is: {code}. Valid for 5 minutes.";
            await SendSmsAsync(phoneNumber, message);
        }
    }
}

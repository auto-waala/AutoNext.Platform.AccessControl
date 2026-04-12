namespace AutoNext.Platform.AccessControl.API.Managers.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendVerificationEmailAsync(string to, string otpCode);
        Task SendPasswordResetEmailAsync(string to, string otpCode);
        Task SendWelcomeEmailAsync(string to, string name);
    }
}

namespace AutoNext.Platform.AccessControl.API.Managers.Interfaces
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
        Task SendVerificationSmsAsync(string phoneNumber, string otpCode);
        Task SendTwoFactorCodeSmsAsync(string phoneNumber, string code);
    }
}

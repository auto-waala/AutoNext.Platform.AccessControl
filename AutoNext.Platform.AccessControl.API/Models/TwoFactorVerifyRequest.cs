namespace AutoNext.Platform.AccessControl.API.Models
{
    public class TwoFactorVerifyRequest
    {
        public Guid UserId { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}

using AutoNext.Platform.AccessControl.API.Managers.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AutoNext.Platform.AccessControl.API.Managers.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(username ?? senderEmail, password),
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail!, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to: {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to: {To}", to);
                throw;
            }
        }

        public async Task SendVerificationEmailAsync(string to, string otpCode)
        {
            var subject = "Verify Your Email Address - AutoNext";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .otp-code {{ font-size: 32px; font-weight: bold; color: #4CAF50; text-align: center; padding: 20px; letter-spacing: 5px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                        .button {{ display: inline-block; padding: 10px 20px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Welcome to AutoNext!</h1>
                        </div>
                        <div class='content'>
                            <p>Thank you for registering with AutoNext. Please verify your email address using the OTP code below:</p>
                            <div class='otp-code'>{otpCode}</div>
                            <p>This OTP is valid for 10 minutes.</p>
                            <p>If you didn't create an account with AutoNext, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 AutoNext. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string otpCode)
        {
            var subject = "Password Reset Request - AutoNext";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #ff9800; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .otp-code {{ font-size: 32px; font-weight: bold; color: #ff9800; text-align: center; padding: 20px; letter-spacing: 5px; }}
                        .warning {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 5px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Password Reset Request</h1>
                        </div>
                        <div class='content'>
                            <p>We received a request to reset your password. Use the OTP code below to proceed:</p>
                            <div class='otp-code'>{otpCode}</div>
                            <div class='warning'>
                                <strong>⚠️ Security Notice:</strong> This OTP is valid for 10 minutes. If you didn't request this, please ignore this email and ensure your account is secure.
                            </div>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 AutoNext. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string to, string name)
        {
            var subject = "Welcome to AutoNext!";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .feature {{ margin: 20px 0; padding: 15px; background-color: white; border-radius: 5px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Welcome to AutoNext, {name}! 🚗</h1>
                        </div>
                        <div class='content'>
                            <p>We're excited to have you on board! With AutoNext, you can:</p>
                            <div class='feature'>
                                <strong>✓ Browse Thousands of Vehicles</strong><br>
                                Find your dream car, bike, or truck from our extensive inventory.
                            </div>
                            <div class='feature'>
                                <strong>✓ List Your Vehicle for Sale</strong><br>
                                Reach millions of potential buyers instantly.
                            </div>
                            <div class='feature'>
                                <strong>✓ Get Best Prices</strong><br>
                                Compare prices and get the best deals on vehicles.
                            </div>
                            <div class='feature'>
                                <strong>✓ Secure Transactions</strong><br>
                                Safe and secure payment processing.
                            </div>
                            <p>Get started by verifying your email address and completing your profile.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 AutoNext. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendEmailChangeConfirmationAsync(string to, string newEmail)
        {
            var subject = "Email Change Confirmation - AutoNext";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Email Address Changed</h1>
                        </div>
                        <div class='content'>
                            <p>Your email address has been successfully changed to: <strong>{newEmail}</strong></p>
                            <p>If you did not make this change, please contact our support team immediately.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 AutoNext. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendAccountLockedEmailAsync(string to, DateTime unlockTime)
        {
            var subject = "Account Locked - AutoNext";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .warning {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 5px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Account Temporarily Locked</h1>
                        </div>
                        <div class='content'>
                            <p>Your account has been locked due to multiple failed login attempts.</p>
                            <div class='warning'>
                                <strong>🔒 Account Details:</strong><br>
                                Lock Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC<br>
                                Unlock Time: {unlockTime:yyyy-MM-dd HH:mm:ss} UTC
                            </div>
                            <p>You can reset your password using the Forgot Password option or wait until the lock period expires.</p>
                        </div>
                        < div class='footer'>
                            <p>&copy; 2024 AutoNext.All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendTwoFactorCodeEmailAsync(string to, string code)
        {
            var subject = "Your Two-Factor Authentication Code - AutoNext";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #9C27B0; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .otp-code {{ font-size: 36px; font-weight: bold; color: #9C27B0; text-align: center; padding: 20px; letter-spacing: 10px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Two-Factor Authentication</h1>
                        </div>
                        <div class='content'>
                            <p>Use the following code to complete your two-factor authentication:</p>
                            <div class='otp-code'>{code}</div>
                            <p>This code will expire in 5 minutes.</p>
                            <p>If you didn't request this code, please secure your account immediately.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 AutoNext. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(username ?? senderEmail, password),
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail!, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to: {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to: {To}", to);
                throw;
            }
        }
    }
}

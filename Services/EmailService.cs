using System.Net;
using System.Net.Mail;

namespace PricePulse.Services
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

        public async Task SendVerificationEmailAsync(string toEmail, string verificationLink)
        {
            var subject = "Verify Your PricePulse Account";
            var body = $@"<html>
                <head>
                    <style>
                        body {{ font-family: 'Inter', Arial, sans-serif; line-height: 1.6; color: #1a1a1a; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: #5D288A; color: white; padding: 30px; text-align: center; }}
                        .content {{ background: #f8f9fa; padding: 30px; }}
                        .button {{ display: inline-block; background: #5D288A; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; margin: 20px 0; }}
                        .footer {{ background: #F5F5DC; padding: 20px; text-align: center; font-size: 0.9rem; color: #6b7280; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Welcome to PricePulse</h1>
                        </div>
                        <div class='content'>
                            <h2>You are one step closer to creating your PricePulse account!</h2>
                            <p>An E-mail was sent to you with a confirmation link. Please check your e-mail and click on the provided link to continue registration.</p>
                            <a href='{verificationLink}' class='button'>Verify My Account</a>
                            <p>If the button doesn't work, copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; color: #5D288A;'>{verificationLink}</p>
                            <p>This verification link will expire in 24 hours for security reasons.</p>
                        </div>
                        <div class='footer'>
                            <p>This email was sent from noreply@pricepulse.info</p>
                            <p>If you didn't create a PricePulse account, please ignore this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string firstName)
        {
            var subject = "Welcome to PricePulse - Your Account is Ready!";
            var body = $@"<html>
                <head>
                    <style>
                        body {{ font-family: 'Inter', Arial, sans-serif; line-height: 1.6; color: #1a1a1a; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: #5D288A; color: white; padding: 30px; text-align: center; }}
                        .content {{ background: #f8f9fa; padding: 30px; }}
                        .footer {{ background: #F5F5DC; padding: 20px; text-align: center; font-size: 0.9rem; color: #6b7280; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Welcome to PricePulse, {firstName}!</h1>
                        </div>
                        <div class='content'>
                            <h2>Your account is now active</h2>
                            <p>Congratulations! You've successfully created your PricePulse account and can now access our competitive pricing intelligence platform.</p>
                            <h3>What's next?</h3>
                            <ul>
                                <li>Set up your company profiles</li>
                                <li>Add your products for monitoring</li>
                                <li>Configure competitor tracking</li>
                                <li>Start receiving real-time price alerts</li>
                            </ul>
                            <p>If you need any help getting started, our support team is here to assist you.</p>
                        </div>
                        <div class='footer'>
                            <p>This email was sent from noreply@pricepulse.info</p>
                            <p>© 2025 PricePulse. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var subject = "Reset Your PricePulse Password";
            var body = $@"<html>
                <head>
                    <style>
                        body {{ font-family: 'Inter', Arial, sans-serif; line-height: 1.6; color: #1a1a1a; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: #5D288A; color: white; padding: 30px; text-align: center; }}
                        .content {{ background: #f8f9fa; padding: 30px; }}
                        .button {{ display: inline-block; background: #5D288A; color: white; padding: 15px 30px; text-decoration: none; border-radius: 8px; margin: 20px 0; }}
                        .footer {{ background: #F5F5DC; padding: 20px; text-align: center; font-size: 0.9rem; color: #6b7280; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Password Reset Request</h1>
                        </div>
                        <div class='content'>
                            <h2>Reset Your Password</h2>
                            <p>We received a request to reset your PricePulse account password. Click the button below to create a new password:</p>
                            <a href='{resetLink}' class='button'>Reset My Password</a>
                            <p>If the button doesn't work, copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; color: #5D288A;'>{resetLink}</p>
                            <p>This password reset link will expire in 1 hour for security reasons.</p>
                            <p>If you didn't request a password reset, please ignore this email or contact support if you have concerns.</p>
                        </div>
                        <div class='footer'>
                            <p>This email was sent from noreply@pricepulse.info</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // For development/testing, you can configure SMTP settings in appsettings.json
                // In production, you'd use a service like SendGrid, Mailgun, or Amazon SES
                
                var smtpClient = new SmtpClient()
                {
                    Host = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com",
                    Port = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587"),
                    EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true"),
                    Credentials = new NetworkCredential(
                        _configuration["EmailSettings:Username"] ?? "noreply@pricepulse.info",
                        _configuration["EmailSettings:Password"] ?? "your-app-password"
                    )
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        _configuration["EmailSettings:FromEmail"] ?? "noreply@pricepulse.info",
                        _configuration["EmailSettings:FromName"] ?? "PricePulse"
                    ),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                var replyTo = _configuration["EmailSettings:ReplyToEmail"];
                var replyToName = _configuration["EmailSettings:ReplyToName"] ?? replyTo;
                if (!string.IsNullOrWhiteSpace(replyTo))
                {
                    mailMessage.ReplyToList.Clear();
                    mailMessage.ReplyToList.Add(new MailAddress(replyTo, replyToName));
                }

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                // In production, you might want to queue for retry or use a more robust email service
                
                // For development, we'll just log and continue
                // You could also write to a file or database for testing
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "email_logs.txt");
                await File.AppendAllTextAsync(logPath, 
                    $"[{DateTime.Now}] TO: {toEmail}, SUBJECT: {subject}\n" +
                    $"BODY: {body}\n\n---\n\n");
            }
        }
    }
}

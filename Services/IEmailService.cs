namespace PricePulse.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string toEmail, string verificationLink);
        Task SendWelcomeEmailAsync(string toEmail, string firstName);
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
    }
}

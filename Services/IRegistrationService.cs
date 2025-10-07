using PricePulse.Models;

namespace PricePulse.Services
{
    public interface IRegistrationService
    {
        Task<string> StartRegistrationAsync(string firstName, string lastName, string email, string secondaryEmail, string phoneNumber);
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> CompleteAccountSetupAsync(string token, string password);
        Task<bool> CreateCompanyProfileAsync(string token, string companyName, string industry, string website, string description, string address, string city, string country, string zipCode);
        Task<bool> ResendVerificationEmailAsync(string email);
        Task<string> GetRegistrationStatusAsync(string token);
    }

    public class CompanyRegistrationModel
    {
        public string CompanyName { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string? CompanyLocation { get; set; }
        public string? CompanySize { get; set; }
        public string? CompanyWebsite { get; set; }
        public string? CompanyCategory { get; set; }
    }
}

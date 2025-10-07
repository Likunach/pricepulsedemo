using Microsoft.AspNetCore.Identity;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;
using System.Security.Cryptography;
using System.Text;

namespace PricePulse.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IUserDAO _userDAO;
        private readonly IProfileDAO _profileDAO;
        private readonly ICompanyProfileDAO _companyProfileDAO;
        private readonly IAddressDAO _addressDAO;
        private readonly IContactDAO _contactDAO;
        private readonly IRegistrationVerificationDAO _verificationDAO;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegistrationService> _logger;

        public RegistrationService(
            IUserDAO userDAO,
            IProfileDAO profileDAO,
            ICompanyProfileDAO companyProfileDAO,
            IAddressDAO addressDAO,
            IContactDAO contactDAO,
            IRegistrationVerificationDAO verificationDAO,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<RegistrationService> logger)
        {
            _userDAO = userDAO;
            _profileDAO = profileDAO;
            _companyProfileDAO = companyProfileDAO;
            _addressDAO = addressDAO;
            _contactDAO = contactDAO;
            _verificationDAO = verificationDAO;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> StartRegistrationAsync(string firstName, string lastName, string email, string secondaryEmail, string phoneNumber)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userDAO.GetByEmailAsync(email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException("An account with this email already exists.");
                }

                // Create temporary user record (inactive until verified)
                var user = new User
                {
                    Email = email,
                    PasswordHash = "", // Will be set later during account setup
                    RegistrationDate = DateTime.UtcNow,
                    AccountStatus = "Pending" // Pending until verified
                };

                var createdUser = await _userDAO.CreateAsync(user);
                var userId = createdUser.UserId;

                // Create profile
                var profile = new Profile
                {
                    UserId = userId,
                    FirstName = firstName,
                    LastName = lastName
                };

                await _profileDAO.CreateAsync(profile);

                // Create contact information  
                var contact = new Contact
                {
                    UserId = userId,
                    PhoneNumber = phoneNumber,
                    SecondaryEmail = secondaryEmail
                };

                await _contactDAO.CreateAsync(contact);

                // Generate verification token
                var verificationToken = GenerateSecureToken();
                
                // Create verification record
                var verification = new RegistrationVerification
                {
                    UserId = userId,
                    VerificationToken = verificationToken,
                    TokenExpiry = DateTime.UtcNow.AddHours(24),
                    VerificationStatus = "Pending",
                    CreatedDate = DateTime.UtcNow
                };

                await _verificationDAO.CreateAsync(verification);

                // Send verification email
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
                var verificationLink = $"{baseUrl}/Account/VerifyEmail?token={verificationToken}";
                
                await _emailService.SendVerificationEmailAsync(email, verificationLink);

                _logger.LogInformation($"Registration started for {email}, verification token: {verificationToken}");
                
                return verificationToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to start registration for {email}");
                throw;
            }
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            try
            {
                var verification = await _verificationDAO.GetByVerificationCodeAsync(token);
                if (verification == null || verification.TokenExpiry < DateTime.UtcNow)
                {
                    return false;
                }

                // Mark as verified
                verification.IsVerified = true;
                verification.VerifiedDate = DateTime.UtcNow;
                verification.VerificationStatus = "Verified";

                await _verificationDAO.UpdateAsync(verification);

                _logger.LogInformation($"Email verified for token: {token}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to verify email for token: {token}");
                return false;
            }
        }

        public async Task<bool> CompleteAccountSetupAsync(string token, string password)
        {
            try
            {
                var verification = await _verificationDAO.GetByVerificationCodeAsync(token);
                if (verification == null || !verification.IsVerified)
                {
                    return false;
                }

                // Get user and activate account
                var user = await _userDAO.GetByIdAsync(verification.UserId);
                if (user == null)
                {
                    return false;
                }

                // Hash password (you should use proper password hashing)
                user.PasswordHash = HashPassword(password);
                user.AccountStatus = "Active";

                await _userDAO.UpdateAsync(user);

                // Get profile for welcome email
                var profile = await _profileDAO.GetByUserIdAsync(user.UserId);
                await _emailService.SendWelcomeEmailAsync(user.Email, profile?.FirstName ?? "");

                _logger.LogInformation($"Account setup completed for user: {user.UserId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to complete account setup for token: {token}");
                return false;
            }
        }

        public async Task<bool> CreateCompanyProfileAsync(string token, string companyName, string industry, string website, string description, string address, string city, string country, string zipCode)
        {
            try
            {
                // Get verification record to find user
                var verification = await _verificationDAO.GetByVerificationCodeAsync(token);
                if (verification == null)
                {
                    return false;
                }

                // Create company profile
                var companyProfile = new CompanyProfile
                {
                    UserId = verification.UserId,
                    CompanyName = companyName,
                    CompanyWebsite = website,
                    Summary = description,
                    Industry = industry
                };

                var createdCompany = await _companyProfileDAO.CreateAsync(companyProfile);
                var companyId = createdCompany.CompanyProfileId;

                // Create address if provided
                if (!string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(city))
                {
                    var addressEntity = new Address
                    {
                        CompanyProfileId = companyId,
                        AddressLine1 = address,
                        City = city,
                        PostalCode = zipCode,
                        Country = country
                    };

                    await _addressDAO.CreateAsync(addressEntity);
                }

                _logger.LogInformation($"Company profile created for user: {verification.UserId}, company: {companyId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create company profile for token: {token}");
                return false;
            }
        }

        public async Task<bool> ResendVerificationEmailAsync(string email)
        {
            try
            {
                var user = await _userDAO.GetByEmailAsync(email);
                if (user == null)
                {
                    return false;
                }

                var verifications = await _verificationDAO.GetByUserIdAsync(user.UserId);
                var verification = verifications.FirstOrDefault();
                if (verification == null)
                {
                    return false;
                }

                // Generate new token and extend expiry
                verification.VerificationToken = GenerateSecureToken();
                verification.TokenExpiry = DateTime.UtcNow.AddHours(24);
                
                await _verificationDAO.UpdateAsync(verification);

                // Send new verification email
                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5001";
                var verificationLink = $"{baseUrl}/Account/VerifyEmail?token={verification.VerificationToken}";
                
                await _emailService.SendVerificationEmailAsync(email, verificationLink);

                _logger.LogInformation($"Verification email resent to: {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to resend verification email to: {email}");
                return false;
            }
        }

        public async Task<string> GetRegistrationStatusAsync(string token)
        {
            try
            {
                var verification = await _verificationDAO.GetByVerificationCodeAsync(token);
                if (verification == null)
                {
                    return "Invalid";
                }

                if (verification.TokenExpiry < DateTime.UtcNow)
                {
                    return "Expired";
                }

                return verification.VerificationStatus ?? "Pending";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get registration status for token: {token}");
                return "Error";
            }
        }

        private string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        private string HashPassword(string password)
        {
            // In production, use proper password hashing like BCrypt or Identity's hasher
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "PricePulse_Salt"));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

using System;

namespace PricePulse.ViewModels
{
    public class AccountInfoViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? PhoneNumber { get; set; }
        public string? SecondaryEmail { get; set; }
        public string? AvatarUrl { get; set; }

        public string DisplayName => string.IsNullOrWhiteSpace(FirstName + LastName)
            ? Email
            : ($"{FirstName} {LastName}").Trim();
    }
}

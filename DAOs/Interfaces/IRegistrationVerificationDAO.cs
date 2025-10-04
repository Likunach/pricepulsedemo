using PricePulse.Models;

namespace PricePulse.DAOs.Interfaces
{
    public interface IRegistrationVerificationDAO : IBaseDAO<RegistrationVerification>
    {
        // RegistrationVerification-specific operations
        Task<IEnumerable<RegistrationVerification>> GetByUserIdAsync(int userId);
        Task<RegistrationVerification?> GetByVerificationCodeAsync(string verificationCode);
        Task<IEnumerable<RegistrationVerification>> GetPendingVerificationsAsync();
        Task<IEnumerable<RegistrationVerification>> GetExpiredVerificationsAsync();
        Task<bool> VerificationExistsAsync(string verificationCode);
        Task<bool> MarkAsVerifiedAsync(string verificationCode);
        Task<int> CleanupExpiredVerificationsAsync();
        Task<RegistrationVerification?> GetLatestVerificationForUserAsync(int userId);
    }
}

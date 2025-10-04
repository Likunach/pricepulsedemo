using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class RegistrationVerificationDAO : BaseDAO<RegistrationVerification>, IRegistrationVerificationDAO
    {
        public RegistrationVerificationDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<RegistrationVerification>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(rv => rv.UserId == userId)
                .OrderByDescending(rv => rv.CreatedDate)
                .Include(rv => rv.User)
                .ToListAsync();
        }

        public async Task<RegistrationVerification?> GetByVerificationCodeAsync(string verificationCode)
        {
            return await _dbSet
                .Include(rv => rv.User)
                .FirstOrDefaultAsync(rv => rv.VerificationCode == verificationCode);
        }

        public async Task<IEnumerable<RegistrationVerification>> GetPendingVerificationsAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(rv => !rv.IsVerified && rv.ExpiryDate > now)
                .Include(rv => rv.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<RegistrationVerification>> GetExpiredVerificationsAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(rv => !rv.IsVerified && rv.ExpiryDate <= now)
                .Include(rv => rv.User)
                .ToListAsync();
        }

        public async Task<bool> VerificationExistsAsync(string verificationCode)
        {
            return await _dbSet.AnyAsync(rv => rv.VerificationCode == verificationCode);
        }

        public async Task<bool> MarkAsVerifiedAsync(string verificationCode)
        {
            var verification = await _dbSet
                .FirstOrDefaultAsync(rv => rv.VerificationCode == verificationCode);
            
            if (verification == null || verification.IsVerified || verification.ExpiryDate <= DateTime.UtcNow)
                return false;

            verification.IsVerified = true;
            verification.VerifiedDate = DateTime.UtcNow;
            await UpdateAsync(verification);
            return true;
        }

        public async Task<int> CleanupExpiredVerificationsAsync()
        {
            var expiredVerifications = await GetExpiredVerificationsAsync();
            var verificationsList = expiredVerifications.ToList();
            
            if (verificationsList.Any())
            {
                _dbSet.RemoveRange(verificationsList);
                await _context.SaveChangesAsync();
            }

            return verificationsList.Count;
        }

        public async Task<RegistrationVerification?> GetLatestVerificationForUserAsync(int userId)
        {
            return await _dbSet
                .Where(rv => rv.UserId == userId)
                .OrderByDescending(rv => rv.CreatedDate)
                .Include(rv => rv.User)
                .FirstOrDefaultAsync();
        }

        // Override GetAllAsync to include User navigation property
        public override async Task<IEnumerable<RegistrationVerification>> GetAllAsync()
        {
            return await _dbSet
                .Include(rv => rv.User)
                .OrderByDescending(rv => rv.CreatedDate)
                .ToListAsync();
        }
    }
}

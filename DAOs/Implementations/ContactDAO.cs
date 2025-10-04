using Microsoft.EntityFrameworkCore;
using PricePulse.Data;
using PricePulse.DAOs.Interfaces;
using PricePulse.Models;

namespace PricePulse.DAOs.Implementations
{
    public class ContactDAO : BaseDAO<Contact>, IContactDAO
    {
        public ContactDAO(PricePulseDbContext context) : base(context)
        {
        }

        public async Task<Contact?> GetByUserIdAsync(int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Contact?> GetContactWithUserAsync(int contactId)
        {
            return await _dbSet
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ContactId == contactId);
        }

        public async Task<IEnumerable<Contact>> SearchByPhoneNumberAsync(string phoneNumber)
        {
            return await _dbSet
                .Where(c => c.PhoneNumber == phoneNumber)
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task<bool> ContactExistsForUserAsync(int userId)
        {
            return await _dbSet.AnyAsync(c => c.UserId == userId);
        }

        public async Task<bool> UpdateContactInfoAsync(int userId, string? phoneNumber = null, string? alternateEmail = null)
        {
            var contact = await GetByUserIdAsync(userId);
            if (contact == null)
                return false;

            if (phoneNumber != null)
                contact.PhoneNumber = phoneNumber;
            if (alternateEmail != null)
                contact.SecondaryEmail = alternateEmail;

            await UpdateAsync(contact);
            return true;
        }

        // Override GetAllAsync to include User navigation property
        public override async Task<IEnumerable<Contact>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.User)
                .ToListAsync();
        }
    }
}

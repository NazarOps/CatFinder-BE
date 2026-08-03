using ApplicationLayer.SavedAdvertisements.Interfaces;
using DomainLayer.Models;
using DomainLayer.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories.SavedAdvertisements
{
    /// <summary>
    /// EF Core implementation of ISavedAdvertisementRepository.
    /// Extends GenericRepository with an account-scoped lookup for the bookmarks feature.
    /// </summary>
    public class SavedAdvertisementRepository : GenericRepository<SavedAdvertisement>, ISavedAdvertisementRepository
    {
        public SavedAdvertisementRepository(DbContext context) : base(context) { }

        /// <summary>
        /// Returns all advertisements saved/bookmarked by a specific account.
        /// Used to display the user's saved listings page.
        /// </summary>
        public async Task<IEnumerable<SavedAdvertisement>> GetByAccountIdAsync(int accountId)
        {
            return await _dbSet
                .Include(x => x.Advertisement)
                    .ThenInclude(a => a.Cat)
                .Include(x => x.Advertisement)
                    .ThenInclude(a => a.Location)
                .Where(x => x.AccountId == accountId
                    && x.Advertisement.IsVisible
                    && x.Advertisement.ModerationStatus == ModerationStatus.Approved)
                .ToListAsync();
        }

        public async Task<SavedAdvertisement?> GetOwnedByIdAsync(int savedAdvertisementId, int accountId)
        {
            return await _dbSet
                .Include(x => x.Advertisement)
                    .ThenInclude(a => a.Cat)
                .Include(x => x.Advertisement)
                    .ThenInclude(a => a.Location)
                .FirstOrDefaultAsync(x => x.SavedAdvertisementId == savedAdvertisementId
                    && x.AccountId == accountId
                    && x.Advertisement.IsVisible
                    && x.Advertisement.ModerationStatus == ModerationStatus.Approved);
        }
    }
}

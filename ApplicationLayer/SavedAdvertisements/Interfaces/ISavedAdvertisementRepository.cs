using ApplicationLayer.Common.Interfaces;
using DomainLayer.Models;

namespace ApplicationLayer.SavedAdvertisements.Interfaces
{
    /// <summary>
    /// Repository for SavedAdvertisement data access.
    /// Represents the bookmarking feature — a user saving an advertisement to revisit later.
    /// </summary>
    public interface ISavedAdvertisementRepository : IGenericRepository<SavedAdvertisement>
    {
        /// <summary>
        /// Returns all advertisements saved/bookmarked by a specific account.
        /// Used to display a user's saved listings page.
        /// </summary>
        Task<IEnumerable<SavedAdvertisement>> GetByAccountIdAsync(int accountId);

        /// <summary>
        /// Returns a single saved advertisement only when it belongs to the specified account.
        /// </summary>
        Task<SavedAdvertisement?> GetOwnedByIdAsync(int savedAdvertisementId, int accountId);
    }
}

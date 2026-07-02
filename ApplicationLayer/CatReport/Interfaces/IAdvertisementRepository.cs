using ApplicationLayer.Common.Interfaces;
using DomainLayer.Models;

namespace ApplicationLayer.CatReport.Interfaces
{
    /// <summary>
    /// Repository for Advertisement data access.
    /// Inherits standard CRUD from IGenericRepository and adds filtering methods
    /// needed by the GET /api/advertisements query parameters.
    /// </summary>
    public interface IAdvertisementRepository : IGenericRepository<Advertisement>
    {
        /// <summary>
        /// Returns all advertisements posted by a specific account.
        /// Used on a user's profile to show their own listings.
        /// </summary>
        Task<IEnumerable<Advertisement>> GetByAccountIdAsync(int accountId);

        /// <summary>
        /// Filters advertisements by type (Lost or Found).
        /// Maps to GET /api/advertisements?type=Lost.
        /// </summary>
        Task<IEnumerable<Advertisement>> GetByTypeAsync(AdvertisementType type);

        /// <summary>
        /// Filters advertisements by status (Active, Resolved, Closed).
        /// Useful for showing only active listings by default.
        /// </summary>
        Task<IEnumerable<Advertisement>> GetByStatusAsync(AdvertisementStatus status);

        /// <summary>
        /// Combined filter for GET /api/advertisements?type=Lost&amp;city=Göteborg.
        /// Only returns advertisements that are visible and approved by admins.
        /// </summary>
        Task<IEnumerable<Advertisement>> GetFilteredAsync(AdvertisementType? type, string? city, int skip = 0, int take = 12);

        /// <summary>
        /// Admin-only query returning all advertisements regardless of visibility or moderation status.
        /// Supports the same optional type/city filters as GetFilteredAsync.
        /// </summary>
        Task<IEnumerable<Advertisement>> GetAllForAdminAsync(AdvertisementType? type, string? city);
    }
}

using ApplicationLayer.CatReport.Interfaces;
using DomainLayer.Models;
using DomainLayer.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories.Advertisements
{
    /// <summary>
    /// EF Core implementation of IAdvertisementRepository.
    /// Extends GenericRepository with filtering methods that map to the
    /// GET /api/advertisements query parameters.
    /// </summary>
    public class AdvertisementRepository : GenericRepository<Advertisement>, IAdvertisementRepository
    {
        public AdvertisementRepository(DbContext context) : base(context) { }

        /// <summary>
        /// Returns all advertisements posted by a specific account.
        /// Used on a user's profile page to show their own listings.
        /// </summary>
        public async Task<IEnumerable<Advertisement>> GetByAccountIdAsync(int accountId)
            => await _dbSet
                .Include(a => a.Cat)
                .Include(a => a.Location)
                .Where(a => a.AccountId == accountId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

        /// <summary>
        /// Filters by Lost or Found. Maps to GET /api/advertisements?type=Lost.
        /// </summary>
        public async Task<IEnumerable<Advertisement>> GetByTypeAsync(AdvertisementType type)
            => await _dbSet
                .Where(a => a.Type == type
                    && a.IsVisible
                    && a.ModerationStatus == ModerationStatus.Approved)
                .ToListAsync();

        /// <summary>
        /// Filters by advertisement status (Active, Resolved, Closed).
        /// Typically used internally to show only active listings by default.
        /// </summary>
        public async Task<IEnumerable<Advertisement>> GetByStatusAsync(AdvertisementStatus status)
            => await _dbSet
                .Where(a => a.Status == status
                    && a.IsVisible
                    && a.ModerationStatus == ModerationStatus.Approved)
                .ToListAsync();

        /// <summary>
        /// Combined filter used by GET /api/advertisements?type=Lost&amp;city=Göteborg.
        /// Includes Location so the city predicate can be evaluated in the database.
        /// </summary>
        public async Task<IEnumerable<Advertisement>> GetFilteredAsync(AdvertisementType? type, string? city, int skip = 0, int take = 12)
        {
            var query = _dbSet
                .Include(a => a.Cat)
                .Include(a => a.Location)
                .Where(a => a.IsVisible && a.ModerationStatus == ModerationStatus.Approved)
                .AsQueryable();

            if (type.HasValue)
                query = query.Where(a => a.Type == type.Value);

            if (!string.IsNullOrEmpty(city))
                query = query.Where(a => a.Location.City == city);

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IEnumerable<Advertisement>> GetAllForAdminAsync(AdvertisementType? type, string? city)
        {
            var query = _dbSet
                .Include(a => a.Cat)
                .Include(a => a.Location)
                .Include(a => a.Reports)
                .AsQueryable();

            if (type.HasValue)
                query = query.Where(a => a.Type == type.Value);

            if (!string.IsNullOrEmpty(city))
                query = query.Where(a => a.Location.City == city);

            return await query.ToListAsync();
        }

        public override async Task<IEnumerable<Advertisement>> GetAllAsync()
        {
            return await _dbSet
                .Include(a => a.Cat)
                .Include(a => a.Location)
                .ToListAsync();
        }

        public override async Task<Advertisement?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(a => a.Cat)
                .Include(a => a.Location)
                .FirstOrDefaultAsync(a => a.AdvertisementId == id);
        }
    }
}

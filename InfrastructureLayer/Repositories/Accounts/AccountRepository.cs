using ApplicationLayer.Common.Security;
using ApplicationLayer.Users.Interfaces;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories.Accounts
{
    /// <summary>
    /// EF Core implementation of IAccountRepository.
    /// Extends GenericRepository with email and username lookups needed for auth flows.
    /// </summary>
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        public AccountRepository(DbContext context) : base(context) { }

        /// <summary>
        /// Case-sensitive email lookup. Used during login, registration duplicate check,
        /// and the forgot-password / reset-password flow.
        /// </summary>
        public async Task<Account?> GetByEmailAsync(string email)
            => await _dbSet.FirstOrDefaultAsync(a => a.Email == email);

        /// <summary>
        /// Case-sensitive username lookup. Used during login and registration
        /// to prevent duplicate usernames.
        /// </summary>
        public async Task<Account?> GetByUsernameAsync(string username)
            => await _dbSet.FirstOrDefaultAsync(a => a.Username == username);

        /// <summary>
        /// Finds an account by its stored refresh token value.
        /// Used during token rotation to validate the incoming refresh token.
        /// </summary>
        public async Task<Account?> GetByRefreshTokenAsync(string refreshToken)
        {
            var refreshTokenHash = TokenHasher.Hash(refreshToken);
            var account = await _dbSet.FirstOrDefaultAsync(a => a.RefreshToken == refreshTokenHash);
            if (account is not null)
            {
                return account;
            }

            // Temporary compatibility path for sessions issued before refresh tokens were hashed at rest.
            return await _dbSet.FirstOrDefaultAsync(a => a.RefreshToken == refreshToken);
        }
    }
}

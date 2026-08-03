using DomainLayer.Models;

namespace ApplicationLayer.Common.Interfaces
{
    /// <summary>
    /// Reads the authenticated user's identity from the current HTTP request.
    /// Implemented in InfrastructureLayer using IHttpContextAccessor + JWT claims.
    /// Handlers inject this to set AccountId without the controller passing it in the body.
    /// </summary>
    public interface IUserContextService
    {
        /// <summary>AccountId from the JWT token, or null if the request is not authenticated.</summary>
        int? AccountId { get; }

        /// <summary>User role from the JWT token, or null if the request is not authenticated.</summary>
        Role? Role { get; }

        /// <summary>True when the authenticated user has the Admin role.</summary>
        bool IsAdmin { get; }
    }
}

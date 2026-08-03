using System.Security.Claims;
using ApplicationLayer.Common.Interfaces;
using DomainLayer.Models;
using Microsoft.AspNetCore.Http;

namespace InfrastructureLayer.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? AccountId
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?.User
                    .FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(value, out var id) ? id : null;
            }
        }

        public Role? Role
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?.User
                    .FindFirstValue(ClaimTypes.Role);

                return Enum.TryParse<Role>(value, out var role) ? role : null;
            }
        }

        public bool IsAdmin => Role == DomainLayer.Models.Role.Admin;
    }
}

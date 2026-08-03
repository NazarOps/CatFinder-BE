using ApplicationLayer.Auth.DTOs;
using ApplicationLayer.Auth.Interfaces;
using ApplicationLayer.Common.Security;
using ApplicationLayer.Users.Interfaces;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, OperationResult<AuthResponseDto>>
    {
        private readonly IAccountRepository _repo;
        private readonly IAuthService _authService;

        public RefreshTokenCommandHandler(IAccountRepository repo, IAuthService authService)
        {
            _repo = repo;
            _authService = authService;
        }

        public async Task<OperationResult<AuthResponseDto>> Handle(
            RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var account = await _repo.GetByRefreshTokenAsync(request.Token);

            if (account is null || account.RefreshTokenExpiresAt < DateTime.UtcNow)
                return OperationResult<AuthResponseDto>.Failure("Invalid or expired refresh token.");

            var (token, expiresAt) = _authService.GenerateJwtToken(account);
            var refreshToken = _authService.GenerateRefreshToken();
            account.RefreshToken = TokenHasher.Hash(refreshToken);
            account.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            await _repo.UpdateAsync(account);

            return OperationResult<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                AccountId = account.AccountId,
                Username = account.Username,
                Email = account.Email,
                Role = account.Role.ToString()
            });
        }
    }
}

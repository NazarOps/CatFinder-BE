using ApplicationLayer.Auth.DTOs;
using ApplicationLayer.Auth.Interfaces;
using ApplicationLayer.Common.Security;
using ApplicationLayer.Users.Interfaces;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, OperationResult<AuthResponseDto>>
    {
        private readonly IAccountRepository _repo;
        private readonly IAuthService _authService;

        public LoginCommandHandler(IAccountRepository repo, IAuthService authService)
        {
            _repo = repo;
            _authService = authService;
        }

        public async Task<OperationResult<AuthResponseDto>> Handle(
            LoginCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var account = await _repo.GetByEmailAsync(dto.UsernameOrEmail)
                ?? await _repo.GetByUsernameAsync(dto.UsernameOrEmail);

            if (account is null || !_authService.VerifyPassword(dto.Password, account.PasswordHash))
                return OperationResult<AuthResponseDto>.Failure("Invalid credentials.");

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

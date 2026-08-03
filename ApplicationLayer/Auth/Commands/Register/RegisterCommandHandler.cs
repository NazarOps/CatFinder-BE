using ApplicationLayer.Auth.DTOs;
using ApplicationLayer.Auth.Interfaces;
using ApplicationLayer.Common.Security;
using ApplicationLayer.Users.Interfaces;
using DomainLayer.Models;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, OperationResult<AuthResponseDto>>
    {
        private readonly IAccountRepository _repo;
        private readonly IAuthService _authService;

        public RegisterCommandHandler(IAccountRepository repo, IAuthService authService)
        {
            _repo = repo;
            _authService = authService;
        }

        public async Task<OperationResult<AuthResponseDto>> Handle(
            RegisterCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            if (await _repo.GetByEmailAsync(dto.Email) is not null)
                return OperationResult<AuthResponseDto>.Failure("Email is already in use.");

            if (await _repo.GetByUsernameAsync(dto.Username) is not null)
                return OperationResult<AuthResponseDto>.Failure("Username is already taken.");

            var account = new Account
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = _authService.HashPassword(dto.Password),
                Role = Role.User
            };

            await _repo.AddAsync(account);

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

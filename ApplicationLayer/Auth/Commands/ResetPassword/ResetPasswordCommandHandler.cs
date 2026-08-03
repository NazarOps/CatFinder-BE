using ApplicationLayer.Auth.Interfaces;
using ApplicationLayer.Common.Security;
using ApplicationLayer.Users.Interfaces;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, OperationResult<bool>>
    {
        private readonly IAccountRepository _repo;
        private readonly IAuthService _authService;

        public ResetPasswordCommandHandler(IAccountRepository repo, IAuthService authService)
        {
            _repo = repo;
            _authService = authService;
        }

        public async Task<OperationResult<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var account = await _repo.GetByEmailAsync(request.Dto.Email);

            if (account is null
                || string.IsNullOrWhiteSpace(account.PasswordResetCode)
                || account.PasswordResetCodeExpiresAt is null
                || account.PasswordResetCodeExpiresAt < DateTime.UtcNow
                || !TokenHasher.Verify(request.Dto.Code, account.PasswordResetCode))
            {
                return OperationResult<bool>.Failure("Invalid or expired reset code.");
            }

            account.PasswordHash = _authService.HashPassword(request.Dto.NewPassword);
            account.PasswordResetCode = null;
            account.PasswordResetCodeExpiresAt = null;
            account.RefreshToken = null;
            account.RefreshTokenExpiresAt = null;
            account.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(account);

            return OperationResult<bool>.Success(true);
        }
    }
}

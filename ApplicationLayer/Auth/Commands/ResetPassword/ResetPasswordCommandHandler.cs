using ApplicationLayer.Auth.Interfaces;
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

            if (account is null)
            {
                return OperationResult<bool>.Failure("No account found with that email address.");
            }

            if (string.IsNullOrWhiteSpace(account.PasswordResetCode))
            {
                return OperationResult<bool>.Failure("No reset code exists for this account.");
            }

            if(account.PasswordResetCode != request.Dto.Code)
            {
                return OperationResult<bool>.Failure("Invalid reset code.");
            }

            if(account.PasswordResetCodeExpiresAt is null || account.PasswordResetCodeExpiresAt < DateTime.UtcNow)
            {
                return OperationResult<bool>.Failure("Reset code has expired");
            }

            account.PasswordHash = _authService.HashPassword(request.Dto.NewPassword);
            account.PasswordResetCode = null;
            account.PasswordResetCodeExpiresAt = null;
            account.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(account);

            return OperationResult<bool>.Success(true);
        }
    }
}

using ApplicationLayer.Users.Interfaces;
using DomainLayer.Models.Common;
using InfrastructureLayer.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace ApplicationLayer.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, OperationResult<bool>>
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(IAccountRepository accountRepository, IEmailService emailService)
        {
            _accountRepository = accountRepository;
            _emailService = emailService;
        }

        public async Task<OperationResult<bool>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetByEmailAsync(request.forgotPasswordDTO.Email);

            if(account == null)
            {
                return OperationResult<bool>.Success(true);
            }

            var code = Random.Shared.Next(100000, 999999).ToString();

            account.PasswordResetCode = code;
            account.PasswordResetCodeExpiresAt = DateTime.UtcNow.AddMinutes(15);
            account.UpdatedAt = DateTime.UtcNow;

            await _accountRepository.UpdateAsync(account);

            var resetUrl = $"http://localhost:5173/resetpassword?email={account.Email}&code={code}";

            await _emailService.SendAsync(
                account.Email,
                "Återställ ditt lösenord - CatFinder",
                $"""
                <h2>Återställ lösenord</h2>

                <p>Din återställningskod är:</p>
                <h1>{code}</h1>

                <p>
                    <a href="{resetUrl}">
                        Klicka här för att återställa ditt lösenord
                    </a>
                </p>

                <p>Koden gäller i 15 minuter.</p>
                <p>Om du inte begärde detta kan du ignorera detta mail.</p>
                """
            );

            return OperationResult<bool>.Success(true);

        }
    }
}

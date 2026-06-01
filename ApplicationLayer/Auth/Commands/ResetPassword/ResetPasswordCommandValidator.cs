using FluentValidation;

namespace ApplicationLayer.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Dto.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Dto.Code)
                .NotEmpty()
                .Length(6);

            RuleFor(x => x.Dto.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(20);
        }
    }
}

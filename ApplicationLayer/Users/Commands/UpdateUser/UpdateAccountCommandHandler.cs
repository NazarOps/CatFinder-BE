using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.Users.DTOs;
using ApplicationLayer.Users.Interfaces;
using AutoMapper;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.Users.Commands.UpdateUser
{
    public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, OperationResult<AccountResponseDto>>
    {
        private readonly IAccountRepository _repo;
        private readonly IUserContextService _userContext;
        private readonly IMapper _mapper;

        public UpdateAccountCommandHandler(
            IAccountRepository repo,
            IUserContextService userContext,
            IMapper mapper)
        {
            _repo = repo;
            _userContext = userContext;
            _mapper = mapper;
        }

        public async Task<OperationResult<AccountResponseDto>> Handle(
            UpdateAccountCommand request,
            CancellationToken cancellationToken)
        {
            var currentAccountId = _userContext.AccountId;
            if (currentAccountId is null)
                return OperationResult<AccountResponseDto>.Failure("User not authenticated.");

            if (!_userContext.IsAdmin && currentAccountId.Value != request.Id)
                return OperationResult<AccountResponseDto>.Failure("Forbidden.");

            var account = await _repo.GetByIdAsync(request.Id);
            if (account is null)
                return OperationResult<AccountResponseDto>.Failure("Account not found.");

            _mapper.Map(request.Dto, account);
            account.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(account);

            return OperationResult<AccountResponseDto>.Success(_mapper.Map<AccountResponseDto>(account));
        }
    }
}

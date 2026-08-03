using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.Users.DTOs;
using ApplicationLayer.Users.Interfaces;
using AutoMapper;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.Users.Queries.GetUserbyId
{
    public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, OperationResult<AccountResponseDto>>
    {
        private readonly IAccountRepository _repo;
        private readonly IUserContextService _userContext;
        private readonly IMapper _mapper;

        public GetAccountByIdQueryHandler(
            IAccountRepository repo,
            IUserContextService userContext,
            IMapper mapper)
        {
            _repo = repo;
            _userContext = userContext;
            _mapper = mapper;
        }

        public async Task<OperationResult<AccountResponseDto>> Handle(
            GetAccountByIdQuery request,
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

            return OperationResult<AccountResponseDto>.Success(_mapper.Map<AccountResponseDto>(account));
        }
    }
}

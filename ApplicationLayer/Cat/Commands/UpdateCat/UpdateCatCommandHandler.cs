using ApplicationLayer.Cat.DTOs;
using ApplicationLayer.Cat.Interfaces;
using ApplicationLayer.Common.Interfaces;
using AutoMapper;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.Cat.Commands.UpdateCat
{
    public class UpdateCatCommandHandler : IRequestHandler<UpdateCatCommand, OperationResult<CatResponseDto>>
    {
        private readonly ICatRepository _repo;
        private readonly IUserContextService _userContext;
        private readonly IMapper _mapper;

        public UpdateCatCommandHandler(
            ICatRepository repo,
            IUserContextService userContext,
            IMapper mapper)
        {
            _repo = repo;
            _userContext = userContext;
            _mapper = mapper;
        }

        public async Task<OperationResult<CatResponseDto>> Handle(
            UpdateCatCommand request,
            CancellationToken cancellationToken)
        {
            var currentAccountId = _userContext.AccountId;
            if (currentAccountId is null)
                return OperationResult<CatResponseDto>.Failure("User not authenticated.");

            var cat = await _repo.GetByIdAsync(request.Id);
            if (cat is null)
                return OperationResult<CatResponseDto>.Failure("Cat not found.");

            if (!_userContext.IsAdmin && cat.AccountId != currentAccountId.Value)
                return OperationResult<CatResponseDto>.Failure("Forbidden.");

            _mapper.Map(request.Dto, cat);
            await _repo.UpdateAsync(cat);

            return OperationResult<CatResponseDto>.Success(_mapper.Map<CatResponseDto>(cat));
        }
    }
}

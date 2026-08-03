using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.SavedAdvertisements.DTOs;
using ApplicationLayer.SavedAdvertisements.Interfaces;
using ApplicationLayer.SavedAdvertisements.Queries;
using AutoMapper;
using MediatR;

namespace ApplicationLayer.SavedAdvertisements.Handlers
{
    public class GetSavedAdvertisementsByAccoundIdQueryHandler : IRequestHandler<GetSavedAdvertisementByAccoundIdQuery, IEnumerable<SavedAdvertisementResponseDto>>
    {
        private readonly ISavedAdvertisementRepository _savedAdvertisementRepository;
        private readonly IUserContextService _userContext;
        private readonly IMapper _mapper;

        public GetSavedAdvertisementsByAccoundIdQueryHandler(
            ISavedAdvertisementRepository savedAdvertisementRepository,
            IUserContextService userContext,
            IMapper mapper)
        {
            _savedAdvertisementRepository = savedAdvertisementRepository;
            _userContext = userContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SavedAdvertisementResponseDto>> Handle(
            GetSavedAdvertisementByAccoundIdQuery request,
            CancellationToken cancellationToken)
        {
            var accountId = _userContext.AccountId;
            if (accountId is null)
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            var saved = await _savedAdvertisementRepository.GetByAccountIdAsync(accountId.Value);
            return _mapper.Map<IEnumerable<SavedAdvertisementResponseDto>>(saved);
        }
    }
}

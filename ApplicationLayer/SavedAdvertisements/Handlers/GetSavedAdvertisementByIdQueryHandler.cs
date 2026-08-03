using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.SavedAdvertisements.DTOs;
using ApplicationLayer.SavedAdvertisements.Interfaces;
using ApplicationLayer.SavedAdvertisements.Queries;
using AutoMapper;
using MediatR;

namespace ApplicationLayer.SavedAdvertisements.Handlers
{
    public class GetSavedAdvertisementByIdQueryHandler : IRequestHandler<GetSavedAdvertisementByIdQuery, SavedAdvertisementResponseDto?>
    {
        private readonly ISavedAdvertisementRepository _savedAdvertisementRepository;
        private readonly IUserContextService _userContext;
        private readonly IMapper _mapper;

        public GetSavedAdvertisementByIdQueryHandler(
            IMapper mapper,
            ISavedAdvertisementRepository savedAdvertisementRepository,
            IUserContextService userContext)
        {
            _savedAdvertisementRepository = savedAdvertisementRepository;
            _userContext = userContext;
            _mapper = mapper;
        }

        public async Task<SavedAdvertisementResponseDto?> Handle(
            GetSavedAdvertisementByIdQuery request,
            CancellationToken cancellationToken)
        {
            var accountId = _userContext.AccountId;
            if (accountId is null)
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }

            var saved = await _savedAdvertisementRepository.GetOwnedByIdAsync(request.Id, accountId.Value);
            return saved is null ? null : _mapper.Map<SavedAdvertisementResponseDto>(saved);
        }
    }
}

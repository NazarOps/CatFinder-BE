using ApplicationLayer.CatReport.DTOs;
using ApplicationLayer.CatReport.Interfaces;
using ApplicationLayer.Common.Interfaces;
using AutoMapper;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.CatReport.Commands.UpdateCatReport
{
    public class UpdateAdvertisementCommandHandler
        : IRequestHandler<UpdateAdvertisementCommand, OperationResult<AdvertisementResponseDto>>
    {
        private readonly IAdvertisementRepository _repo;
        private readonly IUserContextService _userContext;
        private readonly IMapper _mapper;

        public UpdateAdvertisementCommandHandler(
            IAdvertisementRepository repo,
            IUserContextService userContext,
            IMapper mapper)
        {
            _repo = repo;
            _userContext = userContext;
            _mapper = mapper;
        }

        public async Task<OperationResult<AdvertisementResponseDto>> Handle(
            UpdateAdvertisementCommand request,
            CancellationToken cancellationToken)
        {
            var currentAccountId = _userContext.AccountId;
            if (currentAccountId is null)
                return OperationResult<AdvertisementResponseDto>.Failure("User not authenticated.");

            var ad = await _repo.GetByIdAsync(request.Id);
            if (ad is null)
                return OperationResult<AdvertisementResponseDto>.Failure("Advertisement not found.");

            if (!_userContext.IsAdmin && ad.AccountId != currentAccountId.Value)
                return OperationResult<AdvertisementResponseDto>.Failure("Forbidden.");

            _mapper.Map(request.Dto, ad);
            ad.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(ad);

            return OperationResult<AdvertisementResponseDto>.Success(
                _mapper.Map<AdvertisementResponseDto>(ad));
        }
    }
}

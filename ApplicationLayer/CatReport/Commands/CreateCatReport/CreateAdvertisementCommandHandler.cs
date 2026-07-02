using ApplicationLayer.CatReport.DTOs;
using ApplicationLayer.CatReport.Interfaces;
using ApplicationLayer.Common.Interfaces;
using AutoMapper;
using DomainLayer.Models;
using DomainLayer.Models.Common;
using DomainLayer.Models.Enum;
using MediatR;

namespace ApplicationLayer.CatReport.Commands.CreateCatReport
{
    public class CreateAdvertisementCommandHandler
        : IRequestHandler<CreateAdvertisementCommand, OperationResult<AdvertisementResponseDto>>
    {
        private readonly IAdvertisementRepository _repo;
        private readonly IMapper _mapper;
        private readonly IUserContextService _userContext;

        public CreateAdvertisementCommandHandler(
            IAdvertisementRepository repo, IMapper mapper, IUserContextService userContext)
        {
            _repo = repo;
            _mapper = mapper;
            _userContext = userContext;
        }

        public async Task<OperationResult<AdvertisementResponseDto>> Handle(
            CreateAdvertisementCommand request, CancellationToken cancellationToken)
        {
            var accountId = _userContext.AccountId;
            if (accountId is null)
                return OperationResult<AdvertisementResponseDto>.Failure("User not authenticated.");

            var ad = _mapper.Map<Advertisement>(request.Dto);
            ad.AccountId = accountId.Value;
            ad.Status = AdvertisementStatus.Active;
            ad.ModerationStatus = ModerationStatus.Pending;
            ad.IsVisible = false;

            if (ad.Cat is not null)
            {
                ad.Cat.AccountId = accountId.Value;
            }

            await _repo.AddAsync(ad);

            return OperationResult<AdvertisementResponseDto>.Success(
                _mapper.Map<AdvertisementResponseDto>(ad));
        }
    }
}

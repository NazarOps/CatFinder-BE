using ApplicationLayer.AdvertisementImages.DTOs;
using ApplicationLayer.AdvertisementImages.Interfaces;
using ApplicationLayer.CatReport.Interfaces;
using ApplicationLayer.Common.Interfaces;
using AutoMapper;
using DomainLayer.Models;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.AdvertisementImages.Commands.CreateAdvertisementImage
{
    public class CreateAdvertisementImageCommandHandler
        : IRequestHandler<CreateAdvertisementImageCommand, OperationResult<AdvertisementImageResponseDto>>
    {
        private readonly IAdvertisementImageRepository _repo;
        private readonly IAdvertisementRepository _advertisementRepository;
        private readonly IUserContextService _userContext;
        private readonly IMapper _mapper;

        public CreateAdvertisementImageCommandHandler(
            IAdvertisementImageRepository repo,
            IAdvertisementRepository advertisementRepository,
            IUserContextService userContext,
            IMapper mapper)
        {
            _repo = repo;
            _advertisementRepository = advertisementRepository;
            _userContext = userContext;
            _mapper = mapper;
        }

        public async Task<OperationResult<AdvertisementImageResponseDto>> Handle(
            CreateAdvertisementImageCommand request,
            CancellationToken cancellationToken)
        {
            var currentAccountId = _userContext.AccountId;
            if (currentAccountId is null)
                return OperationResult<AdvertisementImageResponseDto>.Failure("User not authenticated.");

            var advertisement = await _advertisementRepository.GetByIdAsync(request.Dto.AdvertisementId);
            if (advertisement is null)
                return OperationResult<AdvertisementImageResponseDto>.Failure("Advertisement not found.");

            if (!_userContext.IsAdmin && advertisement.AccountId != currentAccountId.Value)
                return OperationResult<AdvertisementImageResponseDto>.Failure("Forbidden.");

            var image = _mapper.Map<AdvertisementImage>(request.Dto);
            await _repo.AddAsync(image);

            return OperationResult<AdvertisementImageResponseDto>.Success(
                _mapper.Map<AdvertisementImageResponseDto>(image));
        }
    }
}

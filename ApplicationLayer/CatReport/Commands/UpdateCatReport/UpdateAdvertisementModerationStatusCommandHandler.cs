using ApplicationLayer.CatReport.DTOs;
using ApplicationLayer.CatReport.Interfaces;
using AutoMapper;
using DomainLayer.Models.Enum;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.CatReport.Commands.UpdateCatReport
{
    public class UpdateAdvertisementModerationStatusCommandHandler
        : IRequestHandler<UpdateAdvertisementModerationStatusCommand, OperationResult<AdvertisementResponseDto>>
    {
        private readonly IAdvertisementRepository _repo;
        private readonly IMapper _mapper;

        public UpdateAdvertisementModerationStatusCommandHandler(IAdvertisementRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<OperationResult<AdvertisementResponseDto>> Handle(
            UpdateAdvertisementModerationStatusCommand request, CancellationToken cancellationToken)
        {
            var ad = await _repo.GetByIdAsync(request.Id);
            if (ad is null)
                return OperationResult<AdvertisementResponseDto>.Failure("Advertisement not found.");

            ad.ModerationStatus = request.ModerationStatus;
            ad.IsVisible = request.ModerationStatus == ModerationStatus.Approved;
            ad.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(ad);

            return OperationResult<AdvertisementResponseDto>.Success(
                _mapper.Map<AdvertisementResponseDto>(ad));
        }
    }
}

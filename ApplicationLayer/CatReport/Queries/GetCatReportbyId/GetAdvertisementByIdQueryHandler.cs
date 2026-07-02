using ApplicationLayer.CatReport.DTOs;
using ApplicationLayer.CatReport.Interfaces;
using AutoMapper;
using DomainLayer.Models.Common;
using DomainLayer.Models.Enum;
using MediatR;

namespace ApplicationLayer.CatReport.Queries.GetCatReportbyId
{
    public class GetAdvertisementByIdQueryHandler
        : IRequestHandler<GetAdvertisementByIdQuery, OperationResult<AdvertisementResponseDto>>
    {
        private readonly IAdvertisementRepository _repo;
        private readonly IMapper _mapper;

        public GetAdvertisementByIdQueryHandler(IAdvertisementRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<OperationResult<AdvertisementResponseDto>> Handle(
            GetAdvertisementByIdQuery request, CancellationToken cancellationToken)
        {
            var ad = await _repo.GetByIdAsync(request.Id);
            if (ad is null || !ad.IsVisible || ad.ModerationStatus != ModerationStatus.Approved)
                return OperationResult<AdvertisementResponseDto>.Failure("Advertisement not found.");

            return OperationResult<AdvertisementResponseDto>.Success(
                _mapper.Map<AdvertisementResponseDto>(ad));
        }
    }
}

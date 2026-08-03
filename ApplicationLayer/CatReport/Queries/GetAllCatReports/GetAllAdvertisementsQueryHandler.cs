using ApplicationLayer.CatReport.DTOs;
using ApplicationLayer.CatReport.Interfaces;
using AutoMapper;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.CatReport.Queries.GetAllCatReports
{
    public class GetAllAdvertisementsQueryHandler
        : IRequestHandler<GetAllAdvertisementsQuery, OperationResult<List<PublicAdvertisementResponseDto>>>
    {
        private readonly IAdvertisementRepository _repo;
        private readonly IMapper _mapper;

        public GetAllAdvertisementsQueryHandler(IAdvertisementRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<OperationResult<List<PublicAdvertisementResponseDto>>> Handle(
            GetAllAdvertisementsQuery request, CancellationToken cancellationToken)
        {
            var ads = await _repo.GetFilteredAsync(request.Type, request.City, request.Skip, request.Take);
            return OperationResult<List<PublicAdvertisementResponseDto>>.Success(
                _mapper.Map<List<PublicAdvertisementResponseDto>>(ads));
        }
    }
}

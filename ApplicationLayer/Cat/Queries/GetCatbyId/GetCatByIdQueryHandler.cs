using ApplicationLayer.Cat.DTOs;
using ApplicationLayer.Cat.Interfaces;
using AutoMapper;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.Cat.Queries.GetCatbyId
{
    public class GetCatByIdQueryHandler : IRequestHandler<GetCatByIdQuery, OperationResult<PublicCatResponseDto>>
    {
        private readonly ICatRepository _repo;
        private readonly IMapper _mapper;

        public GetCatByIdQueryHandler(ICatRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<OperationResult<PublicCatResponseDto>> Handle(GetCatByIdQuery request, CancellationToken cancellationToken)
        {
            var cat = await _repo.GetByIdAsync(request.Id);
            if (cat is null)
                return OperationResult<PublicCatResponseDto>.Failure("Cat not found.");

            return OperationResult<PublicCatResponseDto>.Success(_mapper.Map<PublicCatResponseDto>(cat));
        }
    }
}

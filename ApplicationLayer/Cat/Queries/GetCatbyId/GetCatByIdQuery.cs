using ApplicationLayer.Cat.DTOs;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.Cat.Queries.GetCatbyId
{
    public record GetCatByIdQuery(int Id) : IRequest<OperationResult<PublicCatResponseDto>>;
}

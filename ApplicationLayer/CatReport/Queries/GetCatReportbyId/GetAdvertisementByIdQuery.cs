using ApplicationLayer.CatReport.DTOs;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.CatReport.Queries.GetCatReportbyId
{
    public record GetAdvertisementByIdQuery(int Id) : IRequest<OperationResult<PublicAdvertisementResponseDto>>;
}

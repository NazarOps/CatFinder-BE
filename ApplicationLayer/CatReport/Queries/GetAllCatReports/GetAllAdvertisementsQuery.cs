using ApplicationLayer.CatReport.DTOs;
using DomainLayer.Models;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.CatReport.Queries.GetAllCatReports
{
    public record GetAllAdvertisementsQuery(AdvertisementType? Type, string? City, int Skip = 0, int Take = 12)
        : IRequest<OperationResult<List<PublicAdvertisementResponseDto>>>;
}

using ApplicationLayer.CatReport.DTOs;
using DomainLayer.Models.Enum;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.CatReport.Commands.UpdateCatReport
{
    public record UpdateAdvertisementModerationStatusCommand(int Id, ModerationStatus ModerationStatus)
        : IRequest<OperationResult<AdvertisementResponseDto>>;
}

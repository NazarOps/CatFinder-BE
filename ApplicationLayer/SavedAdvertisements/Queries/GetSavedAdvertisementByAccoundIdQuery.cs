using ApplicationLayer.SavedAdvertisements.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.SavedAdvertisements.Queries
{
    public record GetSavedAdvertisementByAccoundIdQuery() : IRequest<IEnumerable<SavedAdvertisementResponseDto>>;
}

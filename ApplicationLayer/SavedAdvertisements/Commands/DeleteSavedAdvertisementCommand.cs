using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.SavedAdvertisements.Commands
{
    public record DeleteSavedAdvertisementCommand(int SavedAdvertisementId) : IRequest;
}

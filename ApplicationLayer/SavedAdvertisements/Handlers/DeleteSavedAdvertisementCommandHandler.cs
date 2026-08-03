using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.SavedAdvertisements.Commands;
using ApplicationLayer.SavedAdvertisements.Interfaces;
using MediatR;

namespace ApplicationLayer.SavedAdvertisements.Handlers
{
    public class DeleteSavedAdvertisementCommandHandler : IRequestHandler<DeleteSavedAdvertisementCommand>
    {
        private readonly ISavedAdvertisementRepository _repo;
        private readonly IUserContextService _userContext;

        public DeleteSavedAdvertisementCommandHandler(
            ISavedAdvertisementRepository repo,
            IUserContextService userContext)
        {
            _repo = repo;
            _userContext = userContext;
        }

        public async Task Handle(DeleteSavedAdvertisementCommand request, CancellationToken cancellationToken)
        {
            var accountId = _userContext.AccountId;
            if (accountId is null) return;

            var entry = await _repo.GetOwnedByIdAsync(request.SavedAdvertisementId, accountId.Value);
            if (entry is null) return;

            await _repo.DeleteAsync(entry);
        }
    }
}

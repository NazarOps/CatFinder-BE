using ApplicationLayer.CatReport.Interfaces;
using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.SavedAdvertisements.Interfaces;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.CatReport.Commands.DeleteCatReport
{
    public class DeleteAdvertisementCommandHandler : IRequestHandler<DeleteAdvertisementCommand, OperationResult<bool>>
    {
        private readonly IAdvertisementRepository _repo;
        private readonly ISavedAdvertisementRepository _savedRepo;
        private readonly IUserContextService _userContext;

        public DeleteAdvertisementCommandHandler(
            IAdvertisementRepository repo,
            ISavedAdvertisementRepository savedRepo,
            IUserContextService userContext)
        {
            _repo = repo;
            _savedRepo = savedRepo;
            _userContext = userContext;
        }

        public async Task<OperationResult<bool>> Handle(
            DeleteAdvertisementCommand request,
            CancellationToken cancellationToken)
        {
            var currentAccountId = _userContext.AccountId;
            if (currentAccountId is null)
                return OperationResult<bool>.Failure("User not authenticated.");

            var ad = await _repo.GetByIdAsync(request.Id);
            if (ad is null)
                return OperationResult<bool>.Failure("Advertisement not found.");

            if (!_userContext.IsAdmin && ad.AccountId != currentAccountId.Value)
                return OperationResult<bool>.Failure("Forbidden.");

            var savedEntries = await _savedRepo.FindAsync(s => s.AdvertisementId == request.Id);
            foreach (var entry in savedEntries)
                await _savedRepo.DeleteAsync(entry);

            await _repo.DeleteAsync(ad);
            return OperationResult<bool>.Success(true);
        }
    }
}

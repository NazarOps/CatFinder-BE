using ApplicationLayer.AdvertisementImages.Interfaces;
using ApplicationLayer.CatReport.Interfaces;
using ApplicationLayer.Common.Interfaces;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.AdvertisementImages.Commands.DeleteAdvertisementImage
{
    public class DeleteAdvertisementImageCommandHandler
        : IRequestHandler<DeleteAdvertisementImageCommand, OperationResult<bool>>
    {
        private readonly IAdvertisementImageRepository _repo;
        private readonly IAdvertisementRepository _advertisementRepository;
        private readonly IUserContextService _userContext;

        public DeleteAdvertisementImageCommandHandler(
            IAdvertisementImageRepository repo,
            IAdvertisementRepository advertisementRepository,
            IUserContextService userContext)
        {
            _repo = repo;
            _advertisementRepository = advertisementRepository;
            _userContext = userContext;
        }

        public async Task<OperationResult<bool>> Handle(
            DeleteAdvertisementImageCommand request,
            CancellationToken cancellationToken)
        {
            var currentAccountId = _userContext.AccountId;
            if (currentAccountId is null)
                return OperationResult<bool>.Failure("User not authenticated.");

            var image = await _repo.GetByIdAsync(request.Id);
            if (image is null)
                return OperationResult<bool>.Failure("Advertisement image not found.");

            var advertisement = await _advertisementRepository.GetByIdAsync(image.AdvertisementId);
            if (advertisement is null)
                return OperationResult<bool>.Failure("Advertisement not found.");

            if (!_userContext.IsAdmin && advertisement.AccountId != currentAccountId.Value)
                return OperationResult<bool>.Failure("Forbidden.");

            await _repo.DeleteAsync(image);
            return OperationResult<bool>.Success(true);
        }
    }
}

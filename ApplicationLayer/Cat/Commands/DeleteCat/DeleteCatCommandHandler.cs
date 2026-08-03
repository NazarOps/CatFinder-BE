using ApplicationLayer.Cat.Interfaces;
using ApplicationLayer.Common.Interfaces;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.Cat.Commands.DeleteCat
{
    public class DeleteCatCommandHandler : IRequestHandler<DeleteCatCommand, OperationResult<bool>>
    {
        private readonly ICatRepository _repo;
        private readonly IUserContextService _userContext;

        public DeleteCatCommandHandler(ICatRepository repo, IUserContextService userContext)
        {
            _repo = repo;
            _userContext = userContext;
        }

        public async Task<OperationResult<bool>> Handle(DeleteCatCommand request, CancellationToken cancellationToken)
        {
            var currentAccountId = _userContext.AccountId;
            if (currentAccountId is null)
                return OperationResult<bool>.Failure("User not authenticated.");

            var cat = await _repo.GetByIdAsync(request.Id);
            if (cat is null)
                return OperationResult<bool>.Failure("Cat not found.");

            if (!_userContext.IsAdmin && cat.AccountId != currentAccountId.Value)
                return OperationResult<bool>.Failure("Forbidden.");

            await _repo.DeleteAsync(cat);
            return OperationResult<bool>.Success(true);
        }
    }
}

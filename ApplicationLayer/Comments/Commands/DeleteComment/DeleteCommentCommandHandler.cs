using ApplicationLayer.Common.Interfaces;
using ApplicationLayer.Comments.Interfaces;
using ApplicationLayer.Reports.Interfaces;
using DomainLayer.Models.Common;
using MediatR;

namespace ApplicationLayer.Comments.Commands.DeleteComment
{
    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, OperationResult<bool>>
    {
        private readonly ICommentRepository _repo;
        private readonly IReportRepository _reportRepo;
        private readonly IUserContextService _userContext;

        public DeleteCommentCommandHandler(
            ICommentRepository repo,
            IReportRepository reportRepo,
            IUserContextService userContext)
        {
            _repo = repo;
            _reportRepo = reportRepo;
            _userContext = userContext;
        }

        public async Task<OperationResult<bool>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var currentAccountId = _userContext.AccountId;
            if (currentAccountId is null)
                return OperationResult<bool>.Failure("User not authenticated.");

            var comment = await _repo.GetByIdAsync(request.Id);
            if (comment is null)
                return OperationResult<bool>.Failure("Comment not found.");

            if (!_userContext.IsAdmin && comment.AccountId != currentAccountId.Value)
                return OperationResult<bool>.Failure("Forbidden.");

            var commentReports = await _reportRepo.FindAsync(r => r.CommentId == request.Id);
            foreach (var report in commentReports)
                await _reportRepo.DeleteAsync(report);

            await _repo.DeleteAsync(comment);
            return OperationResult<bool>.Success(true);
        }
    }
}

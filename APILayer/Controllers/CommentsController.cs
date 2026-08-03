using ApplicationLayer.Comments.Commands.CreateComment;
using ApplicationLayer.Comments.Commands.DeleteComment;
using ApplicationLayer.Comments.DTOs;
using ApplicationLayer.Comments.Queries.GetAllComments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers
{
    // Handles comments on advertisements.
    // Comments are always scoped to an advertisement — you cannot fetch a comment
    // without knowing which advertisement it belongs to.
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ISender _mediator;

        public CommentsController(ISender mediator) => _mediator = mediator;

        // GET /api/advertisements/{advertisementId}/comments
        // Returns all comments for a specific advertisement.
        [HttpGet("/api/advertisements/{advertisementId:int}/comments")]
        [ProducesResponseType(typeof(IEnumerable<CommentResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByAdvertisement(int advertisementId)
        {
            var result = await _mediator.Send(new GetCommentsByAdvertisementQuery(advertisementId));
            return Ok(result);
        }

        // POST /api/advertisements/{advertisementId}/comments
        // Adds a new comment to an advertisement. AccountId is taken from the JWT token.
        [Authorize]
        [HttpPost("/api/advertisements/{advertisementId:int}/comments")]
        [ProducesResponseType(typeof(CommentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(int advertisementId, [FromBody] CreateCommentDto dto)
        {
            dto.AdvertisementId = advertisementId;
            var result = await _mediator.Send(new CreateCommentCommand(dto));
            if (!result.IsSuccess) return BadRequest(result);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        // DELETE /api/comments/{id}
        // Removes a comment by its id. Only the comment author or an admin should be allowed.
        [HttpDelete("{id:int}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteCommentCommand(id));
            if (!result.IsSuccess && result.Errors.Contains("Forbidden."))
                return Forbid();
            if (!result.IsSuccess) return NotFound(result);
            return NoContent();
        }
    }
}

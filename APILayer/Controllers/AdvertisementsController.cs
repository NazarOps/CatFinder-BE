using ApplicationLayer.CatReport.Commands.CreateCatReport;
using ApplicationLayer.CatReport.Commands.DeleteCatReport;
using ApplicationLayer.CatReport.Commands.UpdateCatReport;
using ApplicationLayer.CatReport.DTOs;
using ApplicationLayer.CatReport.Queries.GetAllCatReports;
using ApplicationLayer.CatReport.Queries.GetCatReportbyId;
using ApplicationLayer.CatReport.Queries.GetMyAdvertisements;
using DomainLayer.Models;
using DomainLayer.Models.Enum;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers
{
    // Handles Lost and Found cat advertisements.
    // An advertisement always belongs to an account and references a cat and a location.
    [ApiController]
    [Route("api/[controller]")]
    public class AdvertisementsController : ControllerBase
    {
        private readonly ISender _mediator;

        public AdvertisementsController(ISender mediator) => _mediator = mediator;

        // GET /api/advertisements
        // GET /api/advertisements?type=Lost
        // GET /api/advertisements?city=Göteborg
        // Returns all advertisements, with optional filtering by type and/or city.
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PublicAdvertisementResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] AdvertisementType? type,
            [FromQuery] string? city,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 12)
        {
            var result = await _mediator.Send(new GetAllAdvertisementsQuery(type, city, skip, take));
            return Ok(result);
        }

        // GET /api/advertisements/{id}
        // Returns a single advertisement by its id.
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PublicAdvertisementResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetAdvertisementByIdQuery(id));
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }

        // POST /api/advertisements
        // Creates a new Lost or Found advertisement and submits it for admin moderation.
        // Requires an existing CatId and LocationId in the request body.
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(AdvertisementResponseDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateAdvertisementDto dto)
        {
            var result = await _mediator.Send(new CreateAdvertisementCommand(dto));
            if (!result.IsSuccess) return BadRequest(result);
            return Accepted(result);
        }

        // PUT /api/advertisements/{id}
        // Updates the details of an advertisement (title, description, contact info, etc.).
        // Only the owner of the advertisement should be allowed to update it.
        [Authorize]
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(AdvertisementResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAdvertisementDto dto)
        {
            var result = await _mediator.Send(new UpdateAdvertisementCommand(id, dto));
            if (!result.IsSuccess && result.Errors.Contains("Forbidden."))
                return Forbid();
            if (!result.IsSuccess) return result.Errors.Contains("Advertisement not found.")
                ? NotFound(result) : BadRequest(result);
            return Ok(result);
        }

        // PUT /api/advertisements/{id}/status
        // Changes only the status of an advertisement (Active → Resolved or Closed).
        // Kept as a separate endpoint because status change is a distinct user action.
        [Authorize]
        [HttpPut("{id:int}/status")]
        [ProducesResponseType(typeof(AdvertisementResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] AdvertisementStatus status)
        {
            var result = await _mediator.Send(new UpdateAdvertisementStatusCommand(id, status));
            if (!result.IsSuccess && result.Errors.Contains("Forbidden."))
                return Forbid();
            if (!result.IsSuccess) return result.Errors.Contains("Advertisement not found.")
                ? NotFound(result) : BadRequest(result);
            return Ok(result);
        }

        // GET /api/advertisements/my
        // Returns all advertisements created by the currently authenticated user,
        // including hidden ones (so owners can see if their ad was moderated).
        [HttpGet("my")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<AdvertisementResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMine()
        {
            var result = await _mediator.Send(new GetMyAdvertisementsQuery());
            return Ok(result);
        }

        // GET /api/advertisements/admin
        // Admin-only: returns all advertisements regardless of visibility or moderation state.
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<AdvertisementResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAdmin(
            [FromQuery] AdvertisementType? type,
            [FromQuery] string? city)
        {
            var result = await _mediator.Send(new GetAllAdvertisementsAdminQuery(type, city));
            return Ok(result);
        }

        // PUT /api/advertisements/{id}/moderation-status
        // Admin-only: approves, rejects, or returns an advertisement to pending review.
        [HttpPut("{id:int}/moderation-status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AdvertisementResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateModerationStatus(int id, [FromBody] ModerationStatus moderationStatus)
        {
            var result = await _mediator.Send(new UpdateAdvertisementModerationStatusCommand(id, moderationStatus));
            if (!result.IsSuccess) return result.Errors.Contains("Advertisement not found.")
                ? NotFound(result) : BadRequest(result);
            return Ok(result);
        }

        // DELETE /api/advertisements/{id}
        // Removes an advertisement. Only the owner or an admin should be allowed.
        [Authorize]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteAdvertisementCommand(id));
            if (!result.IsSuccess && result.Errors.Contains("Forbidden."))
                return Forbid();
            if (!result.IsSuccess) return NotFound(result);
            return NoContent();
        }
    }
}

using ApplicationLayer.Cat.Commands.CreateCat;
using ApplicationLayer.Cat.Commands.DeleteCat;
using ApplicationLayer.Cat.Commands.UpdateCat;
using ApplicationLayer.Cat.DTOs;
using ApplicationLayer.Cat.Queries.GetCatbyId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers
{
    // Handles cat registration and management.
    // A cat must exist before an advertisement can be created for it.
    [ApiController]
    [Route("api/[controller]")]
    public class CatsController : ControllerBase
    {
        private readonly ISender _mediator;

        public CatsController(ISender mediator) => _mediator = mediator;

        // GET /api/cats/{id}
        // Returns a single cat's details by its id.
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PublicCatResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetCatByIdQuery(id));
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }

        // POST /api/cats
        // Registers a new cat under the authenticated account.
        // The cat must be created before posting a Lost/Found advertisement.
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(CatResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateCatDto dto)
        {
            var result = await _mediator.Send(new CreateCatCommand(dto));
            if (!result.IsSuccess) return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { id = result.Data!.CatId }, result);
        }

        // PUT /api/cats/{id}
        // Updates a cat's information (name, breed, fur colour, chip status, etc.).
        // Only the owner of the cat should be allowed to update it.
        [Authorize]
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(CatResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCatDto dto)
        {
            var result = await _mediator.Send(new UpdateCatCommand(id, dto));
            if (!result.IsSuccess && result.Errors.Contains("Forbidden."))
                return Forbid();
            if (!result.IsSuccess) return result.Errors.Contains("Cat not found.")
                ? NotFound(result) : BadRequest(result);
            return Ok(result);
        }

        // DELETE /api/cats/{id}
        // Removes a cat record. Should also cascade-remove any linked advertisements.
        [Authorize]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteCatCommand(id));
            if (!result.IsSuccess && result.Errors.Contains("Forbidden."))
                return Forbid();
            if (!result.IsSuccess) return NotFound(result);
            return NoContent();
        }
    }
}

using ApplicationLayer.CatReport.Commands.CreateCatReport;
using ApplicationLayer.CatReport.Queries.GetAllCatReports;
using ApplicationLayer.SavedAdvertisements.Commands;
using ApplicationLayer.SavedAdvertisements.DTOs;
using ApplicationLayer.SavedAdvertisements.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SavedAdvertisementsController : ControllerBase
    {
        private readonly ISender _mediator;

        public SavedAdvertisementsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SavedAdvertisementResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<SavedAdvertisementResponseDto>>> GetByAccount()
        {
            var result = await _mediator.Send(new GetSavedAdvertisementByAccoundIdQuery());
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SavedAdvertisementResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SavedAdvertisementResponseDto>> GetById(int id)
        {
            var result = await _mediator.Send(new GetSavedAdvertisementByIdQuery(id));

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SavedAdvertisementResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SavedAdvertisementResponseDto>> Create([FromBody] CreateSavedAdvertisementDto dto)
        {
            var result = await _mediator.Send(new CreateSavedAdvertisementCommand(dto));

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.SavedAdvertisementId },
                result
            );
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id)
        {
            await _mediator.Send(new DeleteSavedAdvertisementCommand(id));

            return NoContent();
        }
    }
}

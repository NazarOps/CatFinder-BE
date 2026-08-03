using ApplicationLayer.Users.Commands.UpdateUser;
using ApplicationLayer.Users.DTOs;
using ApplicationLayer.Users.Queries.GetUserbyId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APILayer.Controllers
{
    // Handles profile management for existing accounts.
    // Authentication (register, login, logout, reset-password) is handled by AuthController.
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly ISender _mediator;

        public AccountsController(ISender mediator) => _mediator = mediator;

        // GET /api/accounts/{id}
        // Returns a single user's public profile. Used to display profile pages.
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AccountResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetAccountByIdQuery(id));
            if (!result.IsSuccess && result.Errors.Contains("Forbidden."))
                return Forbid();
            if (!result.IsSuccess) return NotFound(result);
            return Ok(result);
        }

        // PUT /api/accounts/{id}
        // Updates a user's username or email. Only the account owner should be allowed.
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(AccountResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountDto dto)
        {
            var result = await _mediator.Send(new UpdateAccountCommand(id, dto));
            if (!result.IsSuccess && result.Errors.Contains("Forbidden."))
                return Forbid();
            if (!result.IsSuccess) return result.Errors.Contains("Account not found.")
                ? NotFound(result) : BadRequest(result);
            return Ok(result);
        }
    }
}

using ApplicationLayer.Auth.Commands.ForgotPassword;
using ApplicationLayer.Auth.Commands.Login;
using ApplicationLayer.Auth.Commands.Logout;
using ApplicationLayer.Auth.Commands.RefreshToken;
using ApplicationLayer.Auth.Commands.Register;
using ApplicationLayer.Auth.Commands.ResetPassword;
using ApplicationLayer.Auth.DTOs;
using ApplicationLayer.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace APILayer.Controllers
{
    // Handles account creation and session management.
    // All endpoints here are public except /logout, which requires a valid JWT.
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IUserContextService _userContext;

        public AuthController(ISender mediator, IUserContextService userContext)
        {
            _mediator = mediator;
            _userContext = userContext;
        }

        // POST /api/auth/register
        // Creates a new account and returns a JWT + refresh token.
        [EnableRateLimiting("auth")]
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _mediator.Send(new RegisterCommand(dto));
            if (!result.IsSuccess) return BadRequest(result);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        // POST /api/auth/login
        // Authenticates with username or email + password, returns JWT + refresh token.
        [EnableRateLimiting("auth")]
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _mediator.Send(new LoginCommand(dto));
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        // POST /api/auth/refresh-token
        // Issues a new JWT using a valid, unexpired refresh token. Rotates the refresh token.
        [EnableRateLimiting("auth")]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(refreshToken));
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        // POST /api/auth/logout
        // Clears the refresh token so the session cannot be renewed.
        // Requires a valid JWT — the AccountId is read from the token claims.
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var accountId = _userContext.AccountId;
            if (accountId is null) return Unauthorized();

            var result = await _mediator.Send(new LogoutCommand(accountId.Value));
            if (!result.IsSuccess) return BadRequest(result);
            return NoContent();
        }

        // POST /api/auth/reset-password
        // Replaces the account's password. Looks up the account by email.
        [EnableRateLimiting("auth")]
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto forgotPasswordDTO)
        {
            var result = await _mediator.Send(new ResetPasswordCommand(forgotPasswordDTO));

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return NoContent();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDTO)
        {
            var result = await _mediator.Send(new ForgotPasswordCommand(forgotPasswordDTO));

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return NoContent();
        }
    }
}

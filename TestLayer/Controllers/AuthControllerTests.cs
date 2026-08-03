using APILayer.Controllers;
using ApplicationLayer.Auth;
using ApplicationLayer.Auth.Commands.Login;
using ApplicationLayer.Auth.Commands.Logout;
using ApplicationLayer.Auth.Commands.RefreshToken;
using ApplicationLayer.Auth.Commands.Register;
using ApplicationLayer.Auth.Commands.ResetPassword;
using ApplicationLayer.Auth.DTOs;
using ApplicationLayer.Common.Interfaces;
using DomainLayer.Models.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;

namespace TestLayer.Controllers;

// Tests for AuthController — covers registration, login, token refresh, logout, and password reset.
// Each test fakes the mediator response so we only check that the controller returns the right HTTP status code.
[TestFixture]
public class AuthControllerTests
{
    private Mock<ISender> _mediator;           // fake version of MediatR — lets us control what it returns
    private Mock<IUserContextService> _userContext; // fake version of the JWT user reader
    private AuthController _controller;        // the real controller we are testing

    // Runs before every test — creates fresh fakes and wires them into the controller.
    [SetUp]
    public void SetUp()
    {
        _mediator = new Mock<ISender>();           // create a blank fake mediator
        _userContext = new Mock<IUserContextService>(); // create a blank fake user context
        _controller = new AuthController(
            _mediator.Object,
            _userContext.Object,
            Options.Create(new JwtSettings { RefreshTokenExpiresInDays = 7 })); // inject the fakes
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // A new user registers successfully → controller should return 201 Created.
    [Test]
    public async Task Register_Returns201_WhenSuccess()
    {
        // Arrange
        _mediator                                           // tell the fake mediator:
            .Setup(m => m.Send(                            //   when Send() is called
                It.IsAny<RegisterCommand>(),               //   with any RegisterCommand
                It.IsAny<CancellationToken>()))            //   and any cancellation token
            .ReturnsAsync(                                 //   then return asynchronously
                OperationResult<AuthResponseDto>.Success(  //   a successful result
                    new AuthResponseDto()));                //   with an empty auth response

        // Act
        var result = await _controller.Register(new RegisterDto()); // call Register with an empty dto

        // Assert
        Assert.That(
            (result as ObjectResult)!.StatusCode,  // cast the result to ObjectResult and read the status code
            Is.EqualTo(StatusCodes.Status201Created)); // it must be 201
    }

    // Registration fails (e.g. email already taken) → controller should return 400 Bad Request.
    [Test]
    public async Task Register_ReturnsBadRequest_WhenFailure()
    {
        // Arrange
        _mediator                                            // tell the fake mediator:
            .Setup(m => m.Send(                             //   when Send() is called
                It.IsAny<RegisterCommand>(),                //   with any RegisterCommand
                It.IsAny<CancellationToken>()))             //   and any cancellation token
            .ReturnsAsync(                                  //   then return asynchronously
                OperationResult<AuthResponseDto>.Failure(   //   a failed result
                    "Email already taken."));               //   with this error message

        // Act
        var result = await _controller.Register(new RegisterDto()); // call Register with an empty dto

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>()); // result must be a 400 Bad Request
    }

    // A user logs in with correct credentials → controller should return 200 OK.
    [Test]
    public async Task Login_ReturnsOk_WhenSuccess()
    {
        // Arrange
        _mediator                                           // tell the fake mediator:
            .Setup(m => m.Send(                            //   when Send() is called
                It.IsAny<LoginCommand>(),                  //   with any LoginCommand
                It.IsAny<CancellationToken>()))            //   and any cancellation token
            .ReturnsAsync(                                 //   then return asynchronously
                OperationResult<AuthResponseDto>.Success(  //   a successful result
                    new AuthResponseDto()));                //   with an empty auth response

        // Act
        var result = await _controller.Login(new LoginDto()); // call Login with an empty dto

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>()); // result must be a 200 OK
    }

    // Login fails (e.g. wrong password) → controller should return 400 Bad Request.
    [Test]
    public async Task Login_ReturnsBadRequest_WhenFailure()
    {
        // Arrange
        _mediator                                            // tell the fake mediator:
            .Setup(m => m.Send(                             //   when Send() is called
                It.IsAny<LoginCommand>(),                   //   with any LoginCommand
                It.IsAny<CancellationToken>()))             //   and any cancellation token
            .ReturnsAsync(                                  //   then return asynchronously
                OperationResult<AuthResponseDto>.Failure(   //   a failed result
                    "Invalid credentials."));               //   with this error message

        // Act
        var result = await _controller.Login(new LoginDto()); // call Login with an empty dto

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>()); // result must be a 400 Bad Request
    }

    // A valid refresh token is sent → controller should return 200 OK with a new token.
    [Test]
    public async Task RefreshToken_ReturnsOk_WhenSuccess()
    {
        // Arrange
        _mediator                                           // tell the fake mediator:
            .Setup(m => m.Send(                            //   when Send() is called
                It.IsAny<RefreshTokenCommand>(),           //   with any RefreshTokenCommand
                It.IsAny<CancellationToken>()))            //   and any cancellation token
            .ReturnsAsync(                                 //   then return asynchronously
                OperationResult<AuthResponseDto>.Success(  //   a successful result
                    new AuthResponseDto()));                //   with an empty auth response

        // Act
        _controller.HttpContext.Request.Headers.Cookie = "catfinder_refresh_token=valid-token";
        var result = await _controller.RefreshToken(); // call RefreshToken with a dummy token cookie

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>()); // result must be a 200 OK
    }

    // An expired or invalid refresh token is sent → controller should return 400 Bad Request.
    [Test]
    public async Task RefreshToken_ReturnsBadRequest_WhenFailure()
    {
        // Arrange
        _mediator                                            // tell the fake mediator:
            .Setup(m => m.Send(                             //   when Send() is called
                It.IsAny<RefreshTokenCommand>(),            //   with any RefreshTokenCommand
                It.IsAny<CancellationToken>()))             //   and any cancellation token
            .ReturnsAsync(                                  //   then return asynchronously
                OperationResult<AuthResponseDto>.Failure(   //   a failed result
                    "Token expired."));                     //   with this error message

        // Act
        _controller.HttpContext.Request.Headers.Cookie = "catfinder_refresh_token=expired-token";
        var result = await _controller.RefreshToken(); // call RefreshToken with an expired token cookie

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>()); // result must be a 400 Bad Request
    }

    // A logged-in user logs out successfully → controller should return 204 No Content.
    [Test]
    public async Task Logout_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        _userContext
            .Setup(u => u.AccountId)  // when AccountId is read from the JWT context
            .Returns(1);              //   pretend the logged-in user has AccountId = 1

        _mediator                                   // tell the fake mediator:
            .Setup(m => m.Send(                    //   when Send() is called
                It.IsAny<LogoutCommand>(),         //   with any LogoutCommand
                It.IsAny<CancellationToken>()))    //   and any cancellation token
            .ReturnsAsync(                         //   then return asynchronously
                OperationResult<bool>.Success(true)); //   a successful result

        // Act
        var result = await _controller.Logout(); // call Logout — the controller reads AccountId internally

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>()); // result must be a 204 No Content
    }

    // Logout is called but there is no user in the JWT token → controller should return 401 Unauthorized.
    [Test]
    public async Task Logout_ReturnsUnauthorized_WhenNoAccountId()
    {
        // Arrange
        _userContext
            .Setup(u => u.AccountId) // when AccountId is read from the JWT context
            .Returns((int?)null);    //   pretend there is no logged-in user (unauthenticated request)

        // Act
        var result = await _controller.Logout(); // call Logout — controller checks AccountId before anything else

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedResult>()); // result must be a 401 Unauthorized
    }

    // A user resets their password successfully → controller should return 204 No Content.
    [Test]
    public async Task ResetPassword_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        _mediator                                   // tell the fake mediator:
            .Setup(m => m.Send(                    //   when Send() is called
                It.IsAny<ResetPasswordCommand>(),  //   with any ResetPasswordCommand
                It.IsAny<CancellationToken>()))    //   and any cancellation token
            .ReturnsAsync(                         //   then return asynchronously
                OperationResult<bool>.Success(true)); //   a successful result

        // Act
        var result = await _controller.ResetPassword(new ResetPasswordDto()); // call ResetPassword with an empty dto

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>()); // result must be a 204 No Content
    }

    // Password reset fails (e.g. email does not exist) → controller should return 400 Bad Request.
    [Test]
    public async Task ResetPassword_ReturnsBadRequest_WhenFailure()
    {
        // Arrange
        _mediator                                   // tell the fake mediator:
            .Setup(m => m.Send(                    //   when Send() is called
                It.IsAny<ResetPasswordCommand>(),  //   with any ResetPasswordCommand
                It.IsAny<CancellationToken>()))    //   and any cancellation token
            .ReturnsAsync(                         //   then return asynchronously
                OperationResult<bool>.Failure(     //   a failed result
                    "Email not found."));          //   with this error message

        // Act
        var result = await _controller.ResetPassword(new ResetPasswordDto()); // call ResetPassword with an empty dto

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>()); // result must be a 400 Bad Request
    }
}

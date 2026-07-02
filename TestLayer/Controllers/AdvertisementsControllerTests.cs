using APILayer.Controllers;
using ApplicationLayer.CatReport.Commands.CreateCatReport;
using ApplicationLayer.CatReport.Commands.DeleteCatReport;
using ApplicationLayer.CatReport.Commands.UpdateCatReport;
using ApplicationLayer.CatReport.DTOs;
using ApplicationLayer.CatReport.Queries.GetAllCatReports;
using ApplicationLayer.CatReport.Queries.GetCatReportbyId;
using DomainLayer.Models;
using DomainLayer.Models.Common;
using DomainLayer.Models.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace TestLayer.Controllers;

// Tests for AdvertisementsController — covers listing, fetching, creating, updating, changing status, and deleting Lost/Found ads.
[TestFixture]
public class AdvertisementsControllerTests
{
    private Mock<ISender> _mediator;             // fake version of MediatR — lets us control what it returns
    private AdvertisementsController _controller; // the real controller we are testing

    // Runs before every test — creates fresh fakes and wires them into the controller.
    [SetUp]
    public void SetUp()
    {
        _mediator = new Mock<ISender>();                                  // create a blank fake mediator
        _controller = new AdvertisementsController(_mediator.Object);     // inject the fake into the controller
    }

    // No filters are applied → controller should return 200 OK with all advertisements.
    [Test]
    public async Task GetAll_ReturnsOk_Always()
    {
        // Arrange
        _mediator                                                            // tell the fake mediator:
            .Setup(m => m.Send(                                             //   when Send() is called
                It.IsAny<GetAllAdvertisementsQuery>(),                      //   with any GetAllAdvertisementsQuery
                It.IsAny<CancellationToken>()))                             //   and any cancellation token
            .ReturnsAsync(                                                  //   then return asynchronously
                OperationResult<List<AdvertisementResponseDto>>.Success([])); //   a successful result with an empty list

        // Act
        var result = await _controller.GetAll(null, null); // call GetAll with no type filter and no city filter

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>()); // result must be a 200 OK
    }

    // Filters for type=Lost and city=Göteborg are applied → controller should still return 200 OK.
    [Test]
    public async Task GetAll_ReturnsOk_WithFilters()
    {
        // Arrange
        _mediator                                                            // tell the fake mediator:
            .Setup(m => m.Send(                                             //   when Send() is called
                It.IsAny<GetAllAdvertisementsQuery>(),                      //   with any GetAllAdvertisementsQuery
                It.IsAny<CancellationToken>()))                             //   and any cancellation token
            .ReturnsAsync(                                                  //   then return asynchronously
                OperationResult<List<AdvertisementResponseDto>>.Success([])); //   a successful result with an empty list

        // Act
        var result = await _controller.GetAll(AdvertisementType.Lost, "Göteborg"); // call GetAll with type and city filters

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>()); // result must be a 200 OK
    }

    // A valid advertisement ID is requested and the ad exists → controller should return 200 OK.
    [Test]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        // Arrange
        _mediator                                                          // tell the fake mediator:
            .Setup(m => m.Send(                                           //   when Send() is called
                It.IsAny<GetAdvertisementByIdQuery>(),                    //   with any GetAdvertisementByIdQuery
                It.IsAny<CancellationToken>()))                           //   and any cancellation token
            .ReturnsAsync(                                                //   then return asynchronously
                OperationResult<AdvertisementResponseDto>.Success(        //   a successful result
                    new AdvertisementResponseDto()));                      //   with an empty advertisement response

        // Act
        var result = await _controller.GetById(1); // call GetById with ID = 1

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>()); // result must be a 200 OK
    }

    // An advertisement ID is requested but no ad exists with that ID → controller should return 404 Not Found.
    [Test]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        _mediator                                                          // tell the fake mediator:
            .Setup(m => m.Send(                                           //   when Send() is called
                It.IsAny<GetAdvertisementByIdQuery>(),                    //   with any GetAdvertisementByIdQuery
                It.IsAny<CancellationToken>()))                           //   and any cancellation token
            .ReturnsAsync(                                                //   then return asynchronously
                OperationResult<AdvertisementResponseDto>.Failure(        //   a failed result
                    "Advertisement not found."));                         //   with this error message

        // Act
        var result = await _controller.GetById(99); // call GetById with an ID that does not exist

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>()); // result must be a 404 Not Found
    }

    // A new advertisement is created successfully → controller should return 201 Created.
    [Test]
    public async Task Create_ReturnsCreated_WhenSuccess()
    {
        // Arrange
        _mediator                                                          // tell the fake mediator:
            .Setup(m => m.Send(                                           //   when Send() is called
                It.IsAny<CreateAdvertisementCommand>(),                   //   with any CreateAdvertisementCommand
                It.IsAny<CancellationToken>()))                           //   and any cancellation token
            .ReturnsAsync(                                                //   then return asynchronously
                OperationResult<AdvertisementResponseDto>.Success(        //   a successful result
                    new AdvertisementResponseDto { AdvertisementId = 1 })); // with AdvertisementId set (needed for the 201 location header)

        // Act
        var result = await _controller.Create(new CreateAdvertisementDto()); // call Create with an empty dto

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>()); // result must be a 201 Created
    }

    // Creating an advertisement fails (e.g. the cat ID does not exist) → controller should return 400 Bad Request.
    [Test]
    public async Task Create_ReturnsBadRequest_WhenFailure()
    {
        // Arrange
        _mediator                                                          // tell the fake mediator:
            .Setup(m => m.Send(                                           //   when Send() is called
                It.IsAny<CreateAdvertisementCommand>(),                   //   with any CreateAdvertisementCommand
                It.IsAny<CancellationToken>()))                           //   and any cancellation token
            .ReturnsAsync(                                                //   then return asynchronously
                OperationResult<AdvertisementResponseDto>.Failure(        //   a failed result
                    "Cat not found."));                                   //   with this error message

        // Act
        var result = await _controller.Create(new CreateAdvertisementDto()); // call Create with an empty dto

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>()); // result must be a 400 Bad Request
    }

    // An advertisement is updated successfully → controller should return 200 OK with the updated data.
    [Test]
    public async Task Update_ReturnsOk_WhenSuccess()
    {
        // Arrange
        _mediator                                                          // tell the fake mediator:
            .Setup(m => m.Send(                                           //   when Send() is called
                It.IsAny<UpdateAdvertisementCommand>(),                   //   with any UpdateAdvertisementCommand
                It.IsAny<CancellationToken>()))                           //   and any cancellation token
            .ReturnsAsync(                                                //   then return asynchronously
                OperationResult<AdvertisementResponseDto>.Success(        //   a successful result
                    new AdvertisementResponseDto()));                      //   with an empty advertisement response

        // Act
        var result = await _controller.Update(1, new UpdateAdvertisementDto()); // call Update with ID = 1 and an empty dto

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>()); // result must be a 200 OK
    }

    // Update is attempted but the advertisement does not exist → controller should return 404 Not Found.
    [Test]
    public async Task Update_ReturnsNotFound_WhenAdvertisementMissing()
    {
        // Arrange
        _mediator                                                          // tell the fake mediator:
            .Setup(m => m.Send(                                           //   when Send() is called
                It.IsAny<UpdateAdvertisementCommand>(),                   //   with any UpdateAdvertisementCommand
                It.IsAny<CancellationToken>()))                           //   and any cancellation token
            .ReturnsAsync(                                                //   then return asynchronously
                OperationResult<AdvertisementResponseDto>.Failure(        //   a failed result
                    "Advertisement not found."));                         //   the controller checks this exact message to decide 404 vs 400

        // Act
        var result = await _controller.Update(99, new UpdateAdvertisementDto()); // call Update with an ID that does not exist

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>()); // result must be a 404 Not Found
    }

    // Update is attempted but the data is invalid (e.g. title is missing) → controller should return 400 Bad Request.
    [Test]
    public async Task Update_ReturnsBadRequest_WhenValidationFails()
    {
        // Arrange
        _mediator                                                          // tell the fake mediator:
            .Setup(m => m.Send(                                           //   when Send() is called
                It.IsAny<UpdateAdvertisementCommand>(),                   //   with any UpdateAdvertisementCommand
                It.IsAny<CancellationToken>()))                           //   and any cancellation token
            .ReturnsAsync(                                                //   then return asynchronously
                OperationResult<AdvertisementResponseDto>.Failure(        //   a failed result
                    "Title is required."));                               //   a validation error (not "not found") → controller returns 400

        // Act
        var result = await _controller.Update(1, new UpdateAdvertisementDto()); // call Update with a valid ID but bad data

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>()); // result must be a 400 Bad Request
    }

    // An advertisement's status is changed successfully (e.g. Active → Resolved) → controller should return 200 OK.
    [Test]
    public async Task UpdateStatus_ReturnsOk_WhenSuccess()
    {
        // Arrange
        _mediator                                                          // tell the fake mediator:
            .Setup(m => m.Send(                                           //   when Send() is called
                It.IsAny<UpdateAdvertisementStatusCommand>(),             //   with any UpdateAdvertisementStatusCommand
                It.IsAny<CancellationToken>()))                           //   and any cancellation token
            .ReturnsAsync(                                                //   then return asynchronously
                OperationResult<AdvertisementResponseDto>.Success(        //   a successful result
                    new AdvertisementResponseDto()));                      //   with an empty advertisement response

        // Act
        var result = await _controller.UpdateStatus(1, AdvertisementStatus.Resolved); // call UpdateStatus with ID = 1 and status = Resolved

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>()); // result must be a 200 OK
    }

    // Status change is attempted but the advertisement does not exist → controller should return 404 Not Found.
    [Test]
    public async Task UpdateStatus_ReturnsNotFound_WhenAdvertisementMissing()
    {
        // Arrange
        _mediator                                                          // tell the fake mediator:
            .Setup(m => m.Send(                                           //   when Send() is called
                It.IsAny<UpdateAdvertisementStatusCommand>(),             //   with any UpdateAdvertisementStatusCommand
                It.IsAny<CancellationToken>()))                           //   and any cancellation token
            .ReturnsAsync(                                                //   then return asynchronously
                OperationResult<AdvertisementResponseDto>.Failure(        //   a failed result
                    "Advertisement not found."));                         //   the controller checks this exact message to decide 404 vs 400

        // Act
        var result = await _controller.UpdateStatus(99, AdvertisementStatus.Resolved); // call UpdateStatus with an ID that does not exist

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>()); // result must be a 404 Not Found
    }

    // An advertisement's moderation status is changed successfully (e.g. Pending â†’ Approved) â†’ controller should return 200 OK.
    [Test]
    public async Task UpdateModerationStatus_ReturnsOk_WhenSuccess()
    {
        // Arrange
        _mediator
            .Setup(m => m.Send(
                It.IsAny<UpdateAdvertisementModerationStatusCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                OperationResult<AdvertisementResponseDto>.Success(
                    new AdvertisementResponseDto()));

        // Act
        var result = await _controller.UpdateModerationStatus(1, ModerationStatus.Approved);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    // Moderation change is attempted but the advertisement does not exist â†’ controller should return 404 Not Found.
    [Test]
    public async Task UpdateModerationStatus_ReturnsNotFound_WhenAdvertisementMissing()
    {
        // Arrange
        _mediator
            .Setup(m => m.Send(
                It.IsAny<UpdateAdvertisementModerationStatusCommand>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                OperationResult<AdvertisementResponseDto>.Failure(
                    "Advertisement not found."));

        // Act
        var result = await _controller.UpdateModerationStatus(99, ModerationStatus.Rejected);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    // An advertisement is deleted successfully â†’ controller should return 204 No Content.
    [Test]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        // Arrange
        _mediator                                   // tell the fake mediator:
            .Setup(m => m.Send(                    //   when Send() is called
                It.IsAny<DeleteAdvertisementCommand>(), //   with any DeleteAdvertisementCommand
                It.IsAny<CancellationToken>()))    //   and any cancellation token
            .ReturnsAsync(                         //   then return asynchronously
                OperationResult<bool>.Success(true)); //   a successful result

        // Act
        var result = await _controller.Delete(1); // call Delete with ID = 1

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>()); // result must be a 204 No Content
    }

    // Delete is attempted but the advertisement does not exist → controller should return 404 Not Found.
    [Test]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        // Arrange
        _mediator                                        // tell the fake mediator:
            .Setup(m => m.Send(                         //   when Send() is called
                It.IsAny<DeleteAdvertisementCommand>(), //   with any DeleteAdvertisementCommand
                It.IsAny<CancellationToken>()))         //   and any cancellation token
            .ReturnsAsync(                              //   then return asynchronously
                OperationResult<bool>.Failure(          //   a failed result
                    "Advertisement not found."));       //   with this error message

        // Act
        var result = await _controller.Delete(99); // call Delete with an ID that does not exist

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>()); // result must be a 404 Not Found
    }
}

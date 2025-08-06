using Beers.API.Controllers.v1.BrewerReview;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Domain.Models.Brewer;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.BrewerReview;

public sealed class CreateBrewerReviewControllerTests
{
    private readonly CreateBrewerReviewController _sut;
    private readonly Mock<ILogger<CreateBrewerReviewController>> _logger = new();
    private readonly Mock<ICreateBrewerReviewService> _createBrewerReviewService = new();

    public CreateBrewerReviewControllerTests()
    {
        _sut = new CreateBrewerReviewController(_logger.Object, _createBrewerReviewService.Object);
    }

    [Fact]
    public async Task PostAsync_bad_request_when_model_is_null()
    {
        var result = await _sut.PostAsync(Guid.NewGuid(), null);
        var objectResult = result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output.Should().Be("Unable to create brewer review because of an invalid input model.");
        }
    }

    [Fact]
    public async Task PostAsync_bad_request_when_model_is_invalid()
    {
        _sut.ModelState.AddModelError("BrewerReview", "Something went wrong");

        var result = await _sut.PostAsync(Guid.NewGuid(), new CreateBrewerReviewModel());
        var objectResult = result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output.Should().Be("Unable to create brewer review because of an invalid input model.");
        }
    }

    [Fact]
    public async Task PostAsync_bad_request_when_model_has_validation_errors()
    {
        _createBrewerReviewService.Setup(x => x.CreateAsync(It.IsAny<CreateBrewerReviewModel>()))
            .ReturnsAsync((new ReadBrewerReviewModel(), [new ValidationFailure { ErrorCode = "01", ErrorMessage = "SomeError" }]));

        var result = await _sut.PostAsync(Guid.NewGuid(), new CreateBrewerReviewModel());
        var objectResult = result as BadRequestObjectResult;
        var output = objectResult?.Value as IEnumerable<string>;

        using (new AssertionScope())
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output.Should().NotBeNull();
            output!.Count().Should().Be(1);
        }
    }

    [Fact]
    public async Task PostAsync_succeeds()
    {
        var brewerId = Guid.NewGuid();
        _createBrewerReviewService.Setup(x => x.CreateAsync(It.IsAny<CreateBrewerReviewModel>()))
            .ReturnsAsync((new ReadBrewerReviewModel { ReviewId = Guid.NewGuid(), BrewerId = brewerId }, []));

        var result = await _sut.PostAsync(brewerId, new CreateBrewerReviewModel());
        var objectResult = result as CreatedAtRouteResult;
        var output = objectResult?.Value as ReadBrewerReviewModel;

        using (new AssertionScope())
        {
            result.Should().BeOfType<CreatedAtRouteResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            objectResult.RouteName.Should().Be("GetBrewerReviewByIdAsync");
            output.Should().NotBeNull();
            output!.ReviewId.Should().NotBe(Guid.Empty);
            output.BrewerId.Should().Be(brewerId);
        }
    }
}

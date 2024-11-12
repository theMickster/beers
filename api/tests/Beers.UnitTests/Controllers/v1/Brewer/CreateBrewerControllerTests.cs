using Beers.API.Controllers.v1.Brewer;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Domain.Models.Brewer;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.Brewer;

public sealed class CreateBrewerControllerTests
{
    private readonly CreateBrewerController _sut;
    private readonly Mock<ILogger<CreateBrewerController>> _logger = new();
    private readonly Mock<ICreateBrewerService> _mockCreateBrewerService = new();

    public CreateBrewerControllerTests()
    {
        _sut = new CreateBrewerController(_logger.Object, _mockCreateBrewerService.Object);
    }

    [Fact]
    public async Task PostAsync_bad_request_when_model_is_null()
    {
        var result = await _sut.PostAsync(null);
        var objectResult = result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("Unable to create brewer because of an invalid input model.");
        }
    }

    [Fact]
    public async Task PostAsync_bad_request_when_model_is_invalid()
    {
        _sut.ModelState.AddModelError("BrewerId", "Something went wrong");

        var result = await _sut.PostAsync(new CreateBrewerModel());
        var objectResult = result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("Unable to create brewer because of an invalid input model.");
        }
    }

    [Fact]
    public async Task PostAsync_bad_request_when_model_has_validation_errors()
    {
        _mockCreateBrewerService.Setup(x => x.CreateAsync(It.IsAny<CreateBrewerModel>()))
            .ReturnsAsync((new ReadBrewerModel(), [new ValidationFailure { ErrorCode = "01", ErrorMessage = "SomeError" }]));

        var result = await _sut.PostAsync(new CreateBrewerModel());
        var objectResult = result as BadRequestObjectResult;
        var output = objectResult?.Value as IEnumerable<string>;

        using (new AssertionScope())
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.ToList().Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task PostAsync_succeeds()
    {
        _mockCreateBrewerService.Setup(x => x.CreateAsync(It.IsAny<CreateBrewerModel>()))
            .ReturnsAsync((new ReadBrewerModel { BrewerId = Guid.NewGuid() }, []));

        var result = await _sut.PostAsync(new CreateBrewerModel());
        var objectResult = result as CreatedAtRouteResult;
        var output = objectResult?.Value as ReadBrewerModel;

        using (new AssertionScope())
        {
            result.Should().BeOfType<CreatedAtRouteResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            output!.Should().NotBeNull();
            output!.BrewerId.Should().NotBe(Guid.Empty);
        }
    }
}

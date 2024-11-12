using Beers.API.Controllers.v1.Beer;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Domain.Models.Beer;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.Beer;

public sealed class CreateBeerControllerTests
{
    private readonly CreateBeerController _sut;
    private readonly Mock<ILogger<CreateBeerController>> _logger = new();
    private readonly Mock<ICreateBeerService> _mockCreateBeerService = new();

    public CreateBeerControllerTests()
    {
        _sut = new CreateBeerController(_logger.Object, _mockCreateBeerService.Object);
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
            output!.Should().Be("Unable to create beer because of an invalid input model.");
        }
    }

    [Fact]
    public async Task PostAsync_bad_request_when_model_is_invalid()
    {
        _sut.ModelState.AddModelError("BeerId", "Something went wrong");

        var result = await _sut.PostAsync(new CreateBeerModel());
        var objectResult = result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("Unable to create beer because of an invalid input model.");
        }
    }

    [Fact]
    public async Task PostAsync_bad_request_when_model_has_validation_errors()
    {
        _mockCreateBeerService.Setup(x => x.CreateAsync(It.IsAny<CreateBeerModel>()))
            .ReturnsAsync((new ReadBeerModel(), [new ValidationFailure { ErrorCode = "01", ErrorMessage = "SomeError" }]));

        var result = await _sut.PostAsync(new CreateBeerModel());
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
        _mockCreateBeerService.Setup(x => x.CreateAsync(It.IsAny<CreateBeerModel>()))
            .ReturnsAsync((new ReadBeerModel{BeerId = Guid.NewGuid(), BrewerId = Guid.NewGuid()}, []));

        var result = await _sut.PostAsync(new CreateBeerModel());
        var objectResult = result as CreatedAtRouteResult;
        var output = objectResult?.Value as ReadBeerModel;

        using (new AssertionScope())
        {
            result.Should().BeOfType<CreatedAtRouteResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            output!.Should().NotBeNull();
            output!.BeerId.Should().NotBe(Guid.Empty);
        }
    }
}

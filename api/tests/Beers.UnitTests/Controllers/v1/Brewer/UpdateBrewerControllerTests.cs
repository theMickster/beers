using AutoMapper;
using Beers.API.Controllers.v1.Brewer;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Domain.Models.Brewer;
using Beers.Domain.Models.Metadata;
using Beers.Domain.Profiles;
using Beers.UnitTests.Common;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.Brewer;

public sealed class UpdateBrewerControllerTests
{
    private readonly UpdateBrewerController _sut;
    private readonly Mock<ILogger<UpdateBrewerController>> _logger = new();
    private readonly Mock<IUpdateBrewerService> _mockUpdateBrewerService = new();
    private readonly Mock<IReadBrewerService> _mockReadBrewerService = new();

    public UpdateBrewerControllerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(BeerCategoryEntityToModelProfile).Assembly)
        );

        var mapper = mappingConfig.CreateMapper();

        _sut = new UpdateBrewerController(_logger.Object, _mockUpdateBrewerService.Object,
            _mockReadBrewerService.Object, mapper);
    }

    [Fact]
    public async Task PatchAsync_bad_request_when_model_is_null()
    {
        var brewerId = Guid.NewGuid();

        var result = await _sut.PatchAsync(brewerId, null!);
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("Invalid patch document");
            _logger.VerifyLoggingMessageIs("Invalid patch document", null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task PatchAsync_not_found()
    {
        var brewerId = Guid.NewGuid();

        _mockReadBrewerService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ReadBrewerModel)null!);

        var result = await _sut.PatchAsync(brewerId, new JsonPatchDocument<UpdateBrewerModel>());
        var objectResult = result.Result as NotFoundResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<NotFoundResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            _logger.VerifyLoggingMessageIs("Unable to locate model.", null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task PatchAsync_bad_request_when_model_has_validation_errors()
    {
        var brewerId = Guid.NewGuid();

        _mockReadBrewerService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync( new ReadBrewerModel());

        _mockUpdateBrewerService.Setup(x => x.UpdateAsync(It.IsAny<UpdateBrewerModel>()))
            .ReturnsAsync((new ReadBrewerModel(), [new ValidationFailure { ErrorCode = "01", ErrorMessage = "SomeError" }]));

        var result = await _sut.PatchAsync(brewerId, new JsonPatchDocument<UpdateBrewerModel>());
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as IEnumerable<string>;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.ToList().Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task PatchAsync_succeeds()
    {
        var brewerId = Guid.NewGuid();

        _mockReadBrewerService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new ReadBrewerModel());

        _mockUpdateBrewerService.Setup(x => x.UpdateAsync(It.IsAny<UpdateBrewerModel>()))
            .ReturnsAsync((new ReadBrewerModel{BrewerId = brewerId }, []));

        var result = await _sut.PatchAsync(brewerId, new JsonPatchDocument<UpdateBrewerModel>());
        var objectResult = result.Result as OkObjectResult;
        var output = objectResult!.Value as ReadBrewerModel;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            output!.BrewerId.Should().Be(brewerId);
        }
    }

    [Fact]
    public async Task PutAsync_bad_request_when_model_is_null()
    {
        var result = await _sut.PutAsync(Guid.NewGuid(), null);
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("Unable to update brewer because of an invalid input model.");
        }
    }

    [Fact]
    public async Task PutAsync_bad_request_when_model_is_invalid()
    {
        _sut.ModelState.AddModelError("BrewerId", "Something went wrong");
        var result = await _sut.PutAsync(Guid.NewGuid(), new UpdateBrewerModel());
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("Unable to update brewer because of an invalid input model.");
        }
    }

    [Fact]
    public async Task PutAsync_bad_request_when_ids_do_not_match()
    {
        var brewerId = Guid.NewGuid();
        var updateId = Guid.NewGuid();
        
        var result = await _sut.PutAsync(updateId, new UpdateBrewerModel { BrewerId = brewerId });
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("The brewer id parameter must match the id of the brewer update request payload.");
        }
    }

    [Fact]
    public async Task PutAsync_bad_request_when_model_has_validation_errors()
    {
        var brewerId = Guid.NewGuid();
        _mockUpdateBrewerService.Setup(x => x.UpdateAsync(It.IsAny<UpdateBrewerModel>()))
            .ReturnsAsync((new ReadBrewerModel(), [new ValidationFailure { ErrorCode = "01", ErrorMessage = "SomeError" }]));

        var result = await _sut.PutAsync(brewerId, new UpdateBrewerModel{BrewerId = brewerId });
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as IEnumerable<string>;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.ToList().Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task PutAsync_succeeds()
    {
        var brewerId = Guid.NewGuid();
        _mockUpdateBrewerService.Setup(x => x.UpdateAsync(It.IsAny<UpdateBrewerModel>()))
            .ReturnsAsync((
                new ReadBrewerModel
                {
                    BrewerId = brewerId, 
                    Name = "A", 
                    BreweryType = new BreweryTypeModel(), 
                    FoundedIn = 2012,
                    CreatedDate = DateTime.UtcNow, 
                    Headquarters = "B", 
                    ModifiedDate = DateTime.UtcNow,
                    Website = "http://c.com"
                }, []));

        var result = await _sut.PutAsync(brewerId, new UpdateBrewerModel{ BrewerId = brewerId });
        var objectResult = result.Result as OkObjectResult;
        var output = objectResult!.Value as ReadBrewerModel;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            output!.Should().NotBeNull();
            output!.BrewerId.Should().NotBe(Guid.Empty);
            output!.Name.Should().Be("A");
            output!.BreweryType.Should().NotBeNull();
            output!.FoundedIn.Should().Be(2012);
            output!.Website.Should().Be("http://c.com");
            output!.CreatedDate.Should().BeOnOrAfter(DateTime.UtcNow.AddSeconds(-2));
            output!.ModifiedDate.Should().BeOnOrAfter(DateTime.UtcNow.AddSeconds(-2));
        }
    }
}

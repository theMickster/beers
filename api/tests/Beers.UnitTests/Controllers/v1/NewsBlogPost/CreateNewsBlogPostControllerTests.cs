using Beers.API.Controllers.v1.NewsBlogPost;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Domain.Models.NewsBlogPost;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.NewsBlogPost;

public sealed class CreateNewsBlogPostControllerTests
{
    private readonly CreateNewsBlogPostController _sut;
    private readonly Mock<ILogger<CreateNewsBlogPostController>> _logger = new();
    private readonly Mock<ICreateNewsBlogPostService> _mockCreateService = new();

    public CreateNewsBlogPostControllerTests()
    {
        _sut = new CreateNewsBlogPostController(_logger.Object, _mockCreateService.Object);
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
            output!.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task PostAsync_bad_request_when_model_is_invalid()
    {
        _sut.ModelState.AddModelError("NewsBlogPostId", "Something went wrong");

        var result = await _sut.PostAsync(Guid.NewGuid(), new CreateNewsBlogPostModel());
        var objectResult = result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task PostAsync_bad_request_when_service_returns_validation_errors()
    {
        _mockCreateService.Setup(x => x.CreateAsync(It.IsAny<CreateNewsBlogPostModel>()))
            .ReturnsAsync((new ReadNewsBlogPostModel(), [new ValidationFailure { ErrorCode = "01", ErrorMessage = "SomeError" }]));

        var result = await _sut.PostAsync(Guid.NewGuid(), new CreateNewsBlogPostModel());
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
        _mockCreateService.Setup(x => x.CreateAsync(It.IsAny<CreateNewsBlogPostModel>()))
            .ReturnsAsync((new ReadNewsBlogPostModel { NewsBlogPostId = Guid.NewGuid(), BrewerId = Guid.NewGuid() }, []));

        var result = await _sut.PostAsync(Guid.NewGuid(), new CreateNewsBlogPostModel());
        var objectResult = result as ObjectResult;
        var output = objectResult?.Value as ReadNewsBlogPostModel;

        using (new AssertionScope())
        {
            result.Should().BeOfType<ObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.Created);
            output!.Should().NotBeNull();
            output!.NewsBlogPostId.Should().NotBe(Guid.Empty);
        }
    }
}

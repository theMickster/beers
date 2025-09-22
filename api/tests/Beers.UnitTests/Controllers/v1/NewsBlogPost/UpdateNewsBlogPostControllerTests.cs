using Beers.API.Controllers.v1.NewsBlogPost;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Domain.Models.NewsBlogPost;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.NewsBlogPost;

public sealed class UpdateNewsBlogPostControllerTests
{
    private readonly UpdateNewsBlogPostController _sut;
    private readonly Mock<ILogger<UpdateNewsBlogPostController>> _logger = new();
    private readonly Mock<IUpdateNewsBlogPostService> _mockUpdateService = new();

    public UpdateNewsBlogPostControllerTests()
    {
        _sut = new UpdateNewsBlogPostController(_logger.Object, _mockUpdateService.Object);
    }

    [Fact]
    public async Task PutAsync_bad_request_when_model_is_null()
    {
        var brewerId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        var result = await _sut.PutAsync(brewerId, postId, null);
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
    public async Task PutAsync_bad_request_when_model_is_invalid()
    {
        _sut.ModelState.AddModelError("NewsBlogPostId", "Something went wrong");

        var brewerId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        var result = await _sut.PutAsync(brewerId, postId, new UpdateNewsBlogPostModel { BrewerId = brewerId, NewsBlogPostId = postId });
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
    public async Task PutAsync_bad_request_when_brewerId_mismatch()
    {
        var brewerId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var differentBrewerId = Guid.NewGuid();

        var result = await _sut.PutAsync(brewerId, postId, new UpdateNewsBlogPostModel { BrewerId = differentBrewerId, NewsBlogPostId = postId });
        var objectResult = result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().Contain("brewerId");
        }
    }

    [Fact]
    public async Task PutAsync_bad_request_when_postId_mismatch()
    {
        var brewerId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var differentPostId = Guid.NewGuid();

        var result = await _sut.PutAsync(brewerId, postId, new UpdateNewsBlogPostModel { BrewerId = brewerId, NewsBlogPostId = differentPostId });
        var objectResult = result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().Contain("postId");
        }
    }

    [Fact]
    public async Task PutAsync_bad_request_when_service_returns_validation_errors()
    {
        var brewerId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        _mockUpdateService.Setup(x => x.UpdateAsync(It.IsAny<UpdateNewsBlogPostModel>()))
            .ReturnsAsync((new ReadNewsBlogPostModel(), [new ValidationFailure { ErrorCode = "01", ErrorMessage = "SomeError" }]));

        var result = await _sut.PutAsync(brewerId, postId, new UpdateNewsBlogPostModel { BrewerId = brewerId, NewsBlogPostId = postId });
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
    public async Task PutAsync_succeeds()
    {
        var brewerId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        _mockUpdateService.Setup(x => x.UpdateAsync(It.IsAny<UpdateNewsBlogPostModel>()))
            .ReturnsAsync((new ReadNewsBlogPostModel { NewsBlogPostId = postId, BrewerId = brewerId }, []));

        var result = await _sut.PutAsync(brewerId, postId, new UpdateNewsBlogPostModel { BrewerId = brewerId, NewsBlogPostId = postId });
        var objectResult = result as OkObjectResult;
        var output = objectResult?.Value as ReadNewsBlogPostModel;

        using (new AssertionScope())
        {
            result.Should().BeOfType<OkObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            output!.Should().NotBeNull();
            output!.NewsBlogPostId.Should().Be(postId);
            output!.BrewerId.Should().Be(brewerId);
        }
    }
}

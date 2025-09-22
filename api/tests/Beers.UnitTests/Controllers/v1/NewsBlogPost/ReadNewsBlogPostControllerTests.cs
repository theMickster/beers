using Beers.API.Controllers.v1.NewsBlogPost;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Domain.Models.NewsBlogPost;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.NewsBlogPost;

public sealed class ReadNewsBlogPostControllerTests
{
    private readonly ReadNewsBlogPostController _sut;
    private readonly Mock<ILogger<ReadNewsBlogPostController>> _logger = new();
    private readonly Mock<IReadNewsBlogPostService> _mockReadNewsBlogPostService = new();

    public ReadNewsBlogPostControllerTests()
    {
        _sut = new ReadNewsBlogPostController(_logger.Object, _mockReadNewsBlogPostService.Object);
    }

    [Fact]
    public async Task GetListAsync_returns_ok_for_empty_list()
    {
        _mockReadNewsBlogPostService.Setup(x => x.GetListAsync(It.IsAny<Guid>())).ReturnsAsync([]);

        var result = await _sut.GetListAsync(Guid.NewGuid());
        var objectResult = result.Result as OkObjectResult;
        var output = objectResult?.Value as IReadOnlyList<ReadNewsBlogPostModel>;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            output.Should().NotBeNull();
            output!.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task GetListAsync_returns_bad_request_for_empty_brewer_id()
    {
        var result = await _sut.GetListAsync(Guid.Empty);
        var objectResult = result.Result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_not_found_when_missing()
    {
        _mockReadNewsBlogPostService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((ReadNewsBlogPostModel?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());
        var objectResult = result.Result as NotFoundResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<NotFoundResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_ok_when_found()
    {
        _mockReadNewsBlogPostService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new ReadNewsBlogPostModel { NewsBlogPostId = Guid.NewGuid(), BrewerId = Guid.NewGuid() });

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());
        var objectResult = result.Result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_bad_request_for_empty_identifier()
    {
        var result = await _sut.GetByIdAsync(Guid.Empty, Guid.NewGuid());
        var objectResult = result.Result as BadRequestObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }
}

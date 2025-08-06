using Beers.API.Controllers.v1.BrewerReview;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Filtering.BrewerReview;
using Beers.Domain.Models.Brewer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.BrewerReview;

public sealed class ReadBrewerReviewControllerTests
{
    private readonly ReadBrewerReviewController _sut;
    private readonly Mock<ILogger<ReadBrewerReviewController>> _logger = new();
    private readonly Mock<IReadBrewerReviewService> _readBrewerReviewService = new();

    public ReadBrewerReviewControllerTests()
    {
        _sut = new ReadBrewerReviewController(_logger.Object, _readBrewerReviewService.Object);
    }

    [Fact]
    public async Task GetListAsync_not_found()
    {
        _readBrewerReviewService.Setup(x => x.GetListAsync(It.IsAny<Guid>())).ReturnsAsync([]);

        var result = await _sut.GetListAsync(Guid.NewGuid());
        var objectResult = result.Result as NotFoundResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<NotFoundResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task GetListAsync_succeeds()
    {
        _readBrewerReviewService.Setup(x => x.GetListAsync(It.IsAny<Guid>()))
            .ReturnsAsync([new ReadBrewerReviewModel { ReviewId = Guid.NewGuid(), BrewerId = Guid.NewGuid() }]);

        var result = await _sut.GetListAsync(Guid.NewGuid());
        var objectResult = result.Result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task GetBrewerReviewByIdAsync_not_found()
    {
        _readBrewerReviewService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((ReadBrewerReviewModel?)null);

        var result = await _sut.GetBrewerReviewByIdAsync(Guid.NewGuid(), Guid.NewGuid());
        var objectResult = result.Result as NotFoundResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<NotFoundResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task GetBrewerReviewByIdAsync_succeeds()
    {
        _readBrewerReviewService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(new ReadBrewerReviewModel { ReviewId = Guid.NewGuid(), BrewerId = Guid.NewGuid() });

        var result = await _sut.GetBrewerReviewByIdAsync(Guid.NewGuid(), Guid.NewGuid());
        var objectResult = result.Result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task SearchAsync_bad_request_when_model_is_invalid()
    {
        _sut.ModelState.AddModelError("Id", "Something went wrong");

        var result = await _sut.SearchAsync(Guid.NewGuid(), new SearchBrewerReviewParameter(), new SearchInputBrewerReviewModel());
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output.Should().Be("Unable to search for brewer reviews because of an invalid input model.");
        }
    }

    [Fact]
    public async Task SearchAsync_succeeds()
    {
        _readBrewerReviewService
            .Setup(x => x.SearchAsync(It.IsAny<SearchBrewerReviewParameter>(), It.IsAny<SearchInputBrewerReviewModel>()))
            .ReturnsAsync(new SearchResultBrewerReviewModel { Results = [] });

        var result = await _sut.SearchAsync(Guid.NewGuid(), new SearchBrewerReviewParameter(), new SearchInputBrewerReviewModel());
        var objectResult = result.Result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }
}

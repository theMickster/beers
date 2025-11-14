using Beers.API.Controllers.v1.NewsBlogPost;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Common.Filtering.NewsBlogPost;
using Beers.Domain.Models.NewsBlogPost;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.NewsBlogPost;

public sealed class SearchNewsBlogPostControllerTests
{
    private readonly SearchNewsBlogPostController _sut;
    private readonly Mock<ILogger<SearchNewsBlogPostController>> _logger = new();
    private readonly Mock<IReadNewsBlogPostService> _mockReadNewsBlogPostService = new();

    public SearchNewsBlogPostControllerTests()
    {
        _sut = new SearchNewsBlogPostController(_logger.Object, _mockReadNewsBlogPostService.Object);
    }

    [Fact]
    public async Task SearchAsync_bad_request_when_model_is_invalid()
    {
        _sut.ModelState.AddModelError("Id", "Something went wrong");

        var result = await _sut.SearchAsync(new SearchNewsBlogPostParameter(), new SearchInputNewsBlogPostModel());
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("Unable to search for news/blog posts because of an invalid input model.");
        }
    }

    [Fact]
    public async Task SearchAsync_succeeds_with_empty_results()
    {
        _mockReadNewsBlogPostService
            .Setup(x => x.SearchAsync(It.IsAny<SearchNewsBlogPostParameter>(), It.IsAny<SearchInputNewsBlogPostModel>()))
            .ReturnsAsync(new SearchResultNewsBlogPostModel { Results = [] });

        var result = await _sut.SearchAsync(new SearchNewsBlogPostParameter(), new SearchInputNewsBlogPostModel());
        var objectResult = result.Result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task SearchAsync_succeeds_with_results()
    {
        var searchResult = new SearchResultNewsBlogPostModel
        {
            Results = [new ReadNewsBlogPostModel { NewsBlogPostId = Guid.NewGuid() }]
        };

        _mockReadNewsBlogPostService
            .Setup(x => x.SearchAsync(It.IsAny<SearchNewsBlogPostParameter>(), It.IsAny<SearchInputNewsBlogPostModel>()))
            .ReturnsAsync(searchResult);

        var result = await _sut.SearchAsync(new SearchNewsBlogPostParameter(), new SearchInputNewsBlogPostModel());
        var objectResult = result.Result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }
}

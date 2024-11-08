using Beers.API.Controllers.v1.BeerCategory;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Domain.Models.Metadata;
using Beers.UnitTests.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.Metadata;

public sealed class ReadBeerCategoryControllerTests
{
    private readonly ReadBeerCategoryController _sut;
    private readonly Mock<IReadBeerCategoryService> _mockReadBeerCategoryService = new();
    private readonly Mock<ILogger<ReadBeerCategoryController>> _logger = new();

    public ReadBeerCategoryControllerTests()
    {
        _sut = new ReadBeerCategoryController(_logger.Object, _mockReadBeerCategoryService.Object);
    }

    [Fact]
    public async Task GetListAsync_returns_not_found()
    {
        _mockReadBeerCategoryService.Setup(x => x.GetListAsync<BeerCategoryModel>())
            .ReturnsAsync(new List<BeerCategoryModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var output = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            output.Should().NotBeNull();
            output!.Should().Be("Unable to locate records the beer category list.");
            _logger.VerifyLoggingMessageIs("Unable to locate records the beer category list.", null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task GetListAsync_returns_okay()
    {
        _mockReadBeerCategoryService.Setup(x => x.GetListAsync<BeerCategoryModel>())
            .ReturnsAsync(MetadataFixtures.GetBeerCategoryModels);

        var result = await _sut.GetListAsync();
        var objectResult = result as OkObjectResult;
        var output = objectResult?.Value as List<BeerCategoryModel>;

        using (new AssertionScope())
        {
            result.Should().BeOfType<OkObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            output!.Should().NotBeNullOrEmpty();
            output!.Count.Should().Be(5);
        }
    }
}

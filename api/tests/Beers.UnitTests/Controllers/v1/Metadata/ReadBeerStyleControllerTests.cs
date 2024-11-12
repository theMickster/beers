using Beers.API.Controllers.v1.BeerStyle;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Domain.Models.Metadata;
using Beers.UnitTests.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.Metadata;

public sealed class ReadBeerStyleControllerTests
{
    private readonly ReadBeerStyleController _sut;
    private readonly Mock<IReadBeerStyleService> _mockReadBeerStyleService = new();
    private readonly Mock<ILogger<ReadBeerStyleController>> _logger = new();

    public ReadBeerStyleControllerTests()
    {
        _sut = new ReadBeerStyleController(_logger.Object, _mockReadBeerStyleService.Object);
    }

    [Fact]
    public async Task GetListAsync_returns_not_found()
    {
        _mockReadBeerStyleService.Setup(x => x.GetListAsync<BeerStyleModel>())
            .ReturnsAsync(new List<BeerStyleModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var output = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            output.Should().NotBeNull();
            output!.Should().Be("Unable to locate records the beer style list.");
            _logger.VerifyLoggingMessageIs("Unable to locate records the beer style list.", null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task GetListAsync_returns_okay()
    {
        _mockReadBeerStyleService.Setup(x => x.GetListAsync<BeerStyleModel>())
            .ReturnsAsync(MetadataFixtures.GetBeerStyleModels);

        var result = await _sut.GetListAsync();
        var objectResult = result as OkObjectResult;
        var output = objectResult?.Value as List<BeerStyleModel>;

        using (new AssertionScope())
        {
            result.Should().BeOfType<OkObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            output!.Should().NotBeNullOrEmpty();
            output!.Count.Should().Be(7);
        }
    }
}


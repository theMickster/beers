using Beers.API.Controllers.v1.BeerType;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Domain.Models.Metadata;
using Beers.UnitTests.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.Metadata;

public sealed class ReadBeerTypeControllerTests
{
    private readonly ReadBeerTypeController _sut;
    private readonly Mock<IReadBeerTypeService> _mockReadBeerTypeService = new();
    private readonly Mock<ILogger<ReadBeerTypeController>> _logger = new();

    public ReadBeerTypeControllerTests()
    {
        _sut = new ReadBeerTypeController(_logger.Object, _mockReadBeerTypeService.Object);
    }

    [Fact]
    public async Task GetListAsync_returns_not_found()
    {
        _mockReadBeerTypeService.Setup(x => x.GetListAsync<BeerTypeModel>())
            .ReturnsAsync(new List<BeerTypeModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var output = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            output.Should().NotBeNull();
            output!.Should().Be("Unable to locate records the beer type list.");
            _logger.VerifyLoggingMessageIs("Unable to locate records the beer type list.", null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task GetListAsync_returns_okay()
    {
        _mockReadBeerTypeService.Setup(x => x.GetListAsync<BeerTypeModel>())
            .ReturnsAsync(MetadataFixtures.GetBeerTypeModels);

        var result = await _sut.GetListAsync();
        var objectResult = result as OkObjectResult;
        var output = objectResult?.Value as List<BeerTypeModel>;

        using (new AssertionScope())
        {
            result.Should().BeOfType<OkObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            output!.Should().NotBeNullOrEmpty();
            output!.Count.Should().Be(3);
        }
    }
}
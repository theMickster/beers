using Beers.API.Controllers.v1.BreweryType;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Domain.Models.Metadata;
using Beers.UnitTests.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.Metadata;

public sealed class ReadBreweryTypeControllerTests
{
    private readonly ReadBreweryTypeController _sut;
    private readonly Mock<IReadBreweryTypeService> _mockReadBreweryTypeService = new();
    private readonly Mock<ILogger<ReadBreweryTypeController>> _logger = new();

    public ReadBreweryTypeControllerTests()
    {
        _sut = new ReadBreweryTypeController(_logger.Object, _mockReadBreweryTypeService.Object);
    }

    [Fact]
    public async Task GetListAsync_returns_not_found()
    {
        _mockReadBreweryTypeService.Setup(x => x.GetListAsync<BreweryTypeModel>())
            .ReturnsAsync(new List<BreweryTypeModel>());

        var result = await _sut.GetListAsync();
        var objectResult = result as NotFoundObjectResult;
        var output = objectResult!.Value! as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);

            output.Should().NotBeNull();
            output!.Should().Be("Unable to locate records the brewery type list.");
            _logger.VerifyLoggingMessageIs("Unable to locate records the brewery type list.", null, LogLevel.Information);
        }
    }

    [Fact]
    public async Task GetListAsync_returns_okay()
    {
        _mockReadBreweryTypeService.Setup(x => x.GetListAsync<BreweryTypeModel>())
            .ReturnsAsync(MetadataFixtures.GetBreweryTypeModels);

        var result = await _sut.GetListAsync();
        var objectResult = result as OkObjectResult;
        var output = objectResult?.Value as List<BreweryTypeModel>;

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

using Beers.API.Controllers.v1.Brewer;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Domain.Models.Brewer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.Brewer;

public sealed class ReadBrewerControllerTests
{
    private readonly ReadBrewerController _sut;
    private readonly Mock<ILogger<ReadBrewerController>> _logger = new();
    private readonly Mock<IReadBrewerService> _mockReadBrewerService = new();

    public ReadBrewerControllerTests()
    {
        _sut = new ReadBrewerController(_logger.Object, _mockReadBrewerService.Object);
    }


    [Fact]
    public async Task GetAsync_not_found()
    {
        _mockReadBrewerService.Setup(x => x.GetListAsync()).ReturnsAsync([]);

        var result = await _sut.GetListAsync();
        var objectResult = result.Result as NotFoundResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<NotFoundResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task GetAsync_succeeds()
    {
        _mockReadBrewerService.Setup(x => x.GetListAsync()).ReturnsAsync([new ReadBrewerModel { BrewerId = Guid.NewGuid() }]);

        var result = await _sut.GetListAsync();
        var objectResult = result.Result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }


    [Fact]
    public async Task GetByIdAsync_not_found()
    {
        _mockReadBrewerService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ReadBrewerModel)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());
        var objectResult = result.Result as NotFoundResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<NotFoundResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task GetByIdAsync_succeeds()
    {
        _mockReadBrewerService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new ReadBrewerModel() { BrewerId = Guid.NewGuid() });

        var result = await _sut.GetByIdAsync(Guid.NewGuid());
        var objectResult = result.Result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }
}

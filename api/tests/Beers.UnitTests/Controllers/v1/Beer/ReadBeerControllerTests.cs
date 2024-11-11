using Beers.API.Controllers.v1.Beer;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Filtering.Beer;
using Beers.Domain.Models.Beer;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.Beer;

public sealed class ReadBeerControllerTests
{
    private readonly ReadBeerController _sut;
    private readonly Mock<ILogger<ReadBeerController>> _logger = new();
    private readonly Mock<IReadBeerService> _mockReadBeerService = new();

    public ReadBeerControllerTests()
    {
        _sut = new ReadBeerController(_logger.Object, _mockReadBeerService.Object);
    }

    [Fact]
    public async Task GetAsync_not_found()
    {
        _mockReadBeerService.Setup(x => x.GetListAsync()).ReturnsAsync([]);

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
        _mockReadBeerService.Setup(x => x.GetListAsync()).ReturnsAsync([new ReadBeerModel(){BeerId = Guid.NewGuid()}]);

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
        _mockReadBeerService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync( (ReadBeerModel)null);

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
        _mockReadBeerService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new ReadBeerModel() { BeerId = Guid.NewGuid() });

        var result = await _sut.GetByIdAsync(Guid.NewGuid());
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

        var result = await _sut.SearchAsync(new SearchBeerParameter(), new SearchInputBeerModel());
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("Unable to search for beers because of an invalid input model.");
        }
    }

    [Fact]
    public async Task SearchAsync_succeeds()
    {
        _mockReadBeerService
            .Setup(x => x.SearchAsync(It.IsAny<SearchBeerParameter>(), It.IsAny<SearchInputBeerModel>()))
            .ReturnsAsync(new SearchResultBeerModel{Results = []});

        var result = await _sut.SearchAsync(new SearchBeerParameter(), new SearchInputBeerModel());
        var objectResult = result.Result as OkObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }
}
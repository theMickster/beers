using Beers.API.Controllers.v1.Beer;
using Beers.Application.Interfaces.Services.Beer;

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

}
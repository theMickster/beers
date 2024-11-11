using Beers.API.Controllers.v1.Brewer;
using Beers.Application.Interfaces.Services.Brewer;

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

}

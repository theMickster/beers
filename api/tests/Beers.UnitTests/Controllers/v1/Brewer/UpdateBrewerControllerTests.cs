using AutoMapper;
using Beers.API.Controllers.v1.Brewer;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Domain.Profiles;

namespace Beers.UnitTests.Controllers.v1.Brewer;

public sealed class UpdateBrewerControllerTests
{
    private readonly UpdateBrewerController _sut;
    private readonly Mock<ILogger<UpdateBrewerController>> _logger = new();
    private readonly Mock<IUpdateBrewerService> _mockUpdateBrewerService = new();
    private readonly Mock<IReadBrewerService> _mockReadBrewerService = new();

    public UpdateBrewerControllerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(BeerCategoryEntityToModelProfile).Assembly)
        );

        var mapper = mappingConfig.CreateMapper();

        _sut = new UpdateBrewerController(_logger.Object, _mockUpdateBrewerService.Object,
            _mockReadBrewerService.Object, mapper);
    }
}

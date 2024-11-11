using AutoMapper;
using Beers.API.Controllers.v1.Beer;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Domain.Profiles;

namespace Beers.UnitTests.Controllers.v1.Beer;

public sealed class UpdateBeerControllerTests
{
    private readonly UpdateBeerController _sut;
    private readonly Mock<ILogger<UpdateBeerController>> _logger = new();
    private readonly Mock<IUpdateBeerService> _mockUpdateBeerService = new();

    public UpdateBeerControllerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(BeerCategoryEntityToModelProfile).Assembly)
        );

        var mapper = mappingConfig.CreateMapper();

        _sut = new UpdateBeerController(_logger.Object, _mockUpdateBeerService.Object);
    }


}

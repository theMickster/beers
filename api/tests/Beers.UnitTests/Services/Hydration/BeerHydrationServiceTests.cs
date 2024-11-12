using Beers.Application.Interfaces.Services.Brewer;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Application.Services.Hydration;

namespace Beers.UnitTests.Services.Hydration;

public sealed class BeerHydrationServiceTests
{
    private readonly BeerHydrationService _sut;
    private readonly Mock<IReadBrewerService> _mockReadBrewerService = new();
    private readonly Mock<IReadBeerTypeService> _mockReadBeerTypeService = new();
    private readonly Mock<IReadBeerStyleService> _mockReadBeerStyleService = new();
    private readonly Mock<IReadBeerCategoryService> _mockReadBeerCategoryService = new();

    public BeerHydrationServiceTests()
    {
        _sut = new BeerHydrationService(_mockReadBrewerService.Object,
            _mockReadBeerTypeService.Object,
            _mockReadBeerStyleService.Object,
            _mockReadBeerCategoryService.Object);
    }

}

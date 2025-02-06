using Beers.Application.Interfaces.Services.Brewer;
using Beers.Application.Validators.Brewer;
using Beers.Domain.Models.Brewer;
using Beers.UnitTests.Common;

namespace Beers.UnitTests.Validators.Brewer;

public sealed class CreateBrewerValidatorTests
{
    private readonly CreateBrewerValidator _sut;
    private readonly Mock<IReadBrewerService> _mockReadBrewerService = new();

    public CreateBrewerValidatorTests()
    {
        _mockReadBrewerService.Setup(x => x.GetListAsync()).ReturnsAsync(BeerFixtures.GetBrewerModels);
        _mockReadBrewerService.Setup(x => x.GetByNameAsync(It.Is<string>(s => s.Contains("New Belgium"))))
            .ReturnsAsync(BeerFixtures.GetBrewerModels().First(x => x.BrewerId == BeerFixtures.BrewerNewBelgium));
        _mockReadBrewerService.Setup(x => x.GetByNameAsync(It.Is<string>(s => !s.Contains("New Belgium"))))
            .ReturnsAsync((ReadBrewerModel)null!);
        _sut = new CreateBrewerValidator(_mockReadBrewerService.Object);
    }

    [Fact]
    public async Task Validator_succeeds()
    {
        var model = new CreateBrewerModel
        {
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeInPlanning),
            FoundedIn = DateTime.UtcNow.Year,
            Headquarters = "On a beach",
            IsDeletable = true,
            Website = "https://beach-bum-brews.com",
            Name = "Beach Bum Brews"
        };

        var result = await _sut.ValidateAsync(model);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    public async Task Validator_fails_name_is_empty(string name)
    {
        var model = new CreateBrewerModel
        {
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeInPlanning),
            FoundedIn = DateTime.UtcNow.Year,
            Headquarters = "On a beach",
            IsDeletable = true,
            Website = "https://beach-bum-brews.com",
            Name = name
        };

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "Name").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_fails_name_is_too_long()
    {
        var model = new CreateBrewerModel
        {
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeInPlanning),
            FoundedIn = DateTime.UtcNow.Year,
            Headquarters = "On a beach",
            IsDeletable = true,
            Website = "https://beach-bum-brews.com",
            Name = StringGenerator.GetRandomString(257)
        };

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "Name").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_fails_name_is_a_duplicate()
    {
        var model = new CreateBrewerModel
        {
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeInPlanning),
            FoundedIn = 1988,
            Headquarters = "Ft. Collins, Colorado",
            IsDeletable = true,
            Website = "https://www.fake-newbelgium.com/",
            Name = "New Belgium"
        };

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "Name").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    public async Task Validator_fails_headquarters_is_empty(string input)
    {
        var model = new CreateBrewerModel
        {
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeInPlanning),
            FoundedIn = DateTime.UtcNow.Year,
            Headquarters = input,
            IsDeletable = true,
            Website = "https://beach-bum-brews.com",
            Name = StringGenerator.GetRandomString(25)
        };

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "Headquarters").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_fails_headquarters_is_too_long()
    {
        var model = new CreateBrewerModel
        {
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeInPlanning),
            FoundedIn = DateTime.UtcNow.Year,
            Headquarters = StringGenerator.GetRandomString(257),
            IsDeletable = true,
            Website = "https://beach-bum-brews.com",
            Name = StringGenerator.GetRandomString(25)
        };

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "Headquarters").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    public async Task Validator_fails_website_is_empty(string input)
    {
        var model = new CreateBrewerModel
        {
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeInPlanning),
            FoundedIn = DateTime.UtcNow.Year,
            Headquarters = "On a beach",
            IsDeletable = true,
            Website = input,
            Name = StringGenerator.GetRandomString(25)
        };

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "Website").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_fails_website_is_too_long()
    {
        var model = new CreateBrewerModel
        {
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeInPlanning),
            FoundedIn = DateTime.UtcNow.Year,
            Headquarters = "On a beach",
            IsDeletable = true,
            Website = StringGenerator.GetRandomString(501),
            Name = StringGenerator.GetRandomString(25)
        };

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "Website").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_fails_website_is_not_secure()
    {
        var model = new CreateBrewerModel
        {
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeInPlanning),
            FoundedIn = DateTime.UtcNow.Year,
            Headquarters = "On a beach",
            IsDeletable = true,
            Website = "http://beach-bum-brews.com",
            Name = StringGenerator.GetRandomString(25)
        };

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "Website").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Theory]
    [InlineData(1499)]
    [InlineData(2050)]
    public async Task Validator_fails_invalid_founding_year(int input)
    {
        var model = new CreateBrewerModel
        {
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeInPlanning),
            FoundedIn = input,
            Headquarters = "On a beach",
            IsDeletable = true,
            Website = "https://beach-bum-brews.com",
            Name = "Beach Bum Brews"
        };

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "FoundedIn").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_fails_BreweryType_is_null()
    {
        var model = new CreateBrewerModel
        {
            BreweryType = null!,
            FoundedIn = DateTime.UtcNow.Year,
            Headquarters = "On a beach",
            IsDeletable = true,
            Website = "https://beach-bum-brews.com",
            Name = StringGenerator.GetRandomString(25)
        };

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "BreweryType").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_fails_BreweryType_id_is_null()
    {
        var model = new CreateBrewerModel
        {
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeInPlanning),
            FoundedIn = DateTime.UtcNow.Year,
            Headquarters = "On a beach",
            IsDeletable = true,
            Website = "https://beach-bum-brews.com",
            Name = StringGenerator.GetRandomString(25)
        };

        model.BreweryType.Id = Guid.Empty;

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "BreweryType").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_fails_BreweryType_name_is_null()
    {
        var model = new CreateBrewerModel
        {
            BreweryType = MetadataFixtures.GetBreweryTypeModels().Single(x => x.Id == MetadataFixtures.BreweryTypeInPlanning),
            FoundedIn = DateTime.UtcNow.Year,
            Headquarters = "On a beach",
            IsDeletable = true,
            Website = "https://beach-bum-brews.com",
            Name = StringGenerator.GetRandomString(25)
        };

        model.BreweryType.Name = null!;

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "BreweryType").Should().BeGreaterThanOrEqualTo(1);
        }
    }
}

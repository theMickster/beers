using Beers.Application.Validators.Beer;
using Beers.Common.Filtering.Beer;

namespace Beers.UnitTests.Validators.Beer;

public sealed class SearchInputBeerModelValidatorTests
{
    private readonly SearchInputBeerModelValidator _sut = new();

    private static List<Guid> Ids(int count) => Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList();

    [Fact]
    public async Task ValidateAsync_passes_for_an_empty_model()
    {
        var result = await _sut.ValidateAsync(new SearchInputBeerModel());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_passes_when_min_price_equals_max_price()
    {
        var result = await _sut.ValidateAsync(new SearchInputBeerModel { MinPrice = 5m, MaxPrice = 5m });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_passes_when_only_one_price_bound_is_supplied()
    {
        (await _sut.ValidateAsync(new SearchInputBeerModel { MinPrice = 5m })).IsValid.Should().BeTrue();
        (await _sut.ValidateAsync(new SearchInputBeerModel { MaxPrice = 0m })).IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_fails_when_min_price_is_greater_than_max_price()
    {
        var result = await _sut.ValidateAsync(new SearchInputBeerModel { MinPrice = 10m, MaxPrice = 5m });

        result.IsValid.Should().BeFalse();
        result.Errors.Any(x => x.PropertyName == "MaxPrice").Should().BeTrue();
    }

    [Theory]
    [InlineData(-0.01, null)]
    [InlineData(null, -1.0)]
    public async Task ValidateAsync_fails_when_a_price_bound_is_negative(double? min, double? max)
    {
        var model = new SearchInputBeerModel
        {
            MinPrice = min.HasValue ? (decimal)min.Value : null,
            MaxPrice = max.HasValue ? (decimal)max.Value : null
        };

        var result = await _sut.ValidateAsync(model);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_passes_when_each_id_list_has_25_items()
    {
        var model = new SearchInputBeerModel { BeerStyleIds = Ids(25), BeerCategoryIds = Ids(25), BeerTypeIds = Ids(25) };

        var result = await _sut.ValidateAsync(model);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(nameof(SearchInputBeerModel.BeerStyleIds))]
    [InlineData(nameof(SearchInputBeerModel.BeerCategoryIds))]
    [InlineData(nameof(SearchInputBeerModel.BeerTypeIds))]
    public async Task ValidateAsync_fails_when_an_id_list_has_more_than_25_items(string property)
    {
        var model = new SearchInputBeerModel();
        typeof(SearchInputBeerModel).GetProperty(property)!.SetValue(model, Ids(26));

        var result = await _sut.ValidateAsync(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Any(x => x.PropertyName == property).Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_passes_when_an_id_list_is_null()
    {
        var model = new SearchInputBeerModel { BeerStyleIds = null! };

        var result = await _sut.ValidateAsync(model);

        result.IsValid.Should().BeTrue();
    }
}

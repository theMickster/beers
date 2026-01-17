using Beers.Application.Validators.Beer;
using Beers.Common.Filtering.Beer;

namespace Beers.UnitTests.Validators.Beer;

public sealed class FacetedSearchInputModelValidatorTests
{
    private readonly FacetedSearchInputModelValidator _sut = new();

    private static List<Guid> Ids(int count) => Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList();

    [Fact]
    public async Task ValidateAsync_passes_for_an_empty_model()
    {
        var result = await _sut.ValidateAsync(new FacetedSearchInputModel());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_passes_when_unknown_ids_are_supplied()
    {
        var model = new FacetedSearchInputModel { BeerStyleIds = Ids(3), BrewerIds = Ids(1) };

        var result = await _sut.ValidateAsync(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_passes_when_min_rating_equals_max_rating()
    {
        var result = await _sut.ValidateAsync(new FacetedSearchInputModel { MinRating = 4m, MaxRating = 4m });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_fails_when_min_rating_is_greater_than_max_rating()
    {
        var result = await _sut.ValidateAsync(new FacetedSearchInputModel { MinRating = 4.5m, MaxRating = 3m });

        result.IsValid.Should().BeFalse();
        result.Errors.Any(x => x.PropertyName == "MaxRating").Should().BeTrue();
    }

    [Theory]
    [InlineData(-0.1, null)]
    [InlineData(5.1, null)]
    [InlineData(null, -1.0)]
    [InlineData(null, 5.5)]
    public async Task ValidateAsync_fails_when_a_rating_bound_is_outside_zero_to_five(double? min, double? max)
    {
        var model = new FacetedSearchInputModel
        {
            MinRating = min.HasValue ? (decimal)min.Value : null,
            MaxRating = max.HasValue ? (decimal)max.Value : null
        };

        var result = await _sut.ValidateAsync(model);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_passes_when_rating_bounds_are_zero_and_five()
    {
        var result = await _sut.ValidateAsync(new FacetedSearchInputModel { MinRating = 0m, MaxRating = 5m });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_passes_when_each_id_list_has_25_items()
    {
        var model = new FacetedSearchInputModel
        {
            BeerStyleIds = Ids(25), BeerCategoryIds = Ids(25), BrewerIds = Ids(25), BeerTypeIds = Ids(25)
        };

        var result = await _sut.ValidateAsync(model);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(nameof(FacetedSearchInputModel.BeerStyleIds))]
    [InlineData(nameof(FacetedSearchInputModel.BeerCategoryIds))]
    [InlineData(nameof(FacetedSearchInputModel.BrewerIds))]
    [InlineData(nameof(FacetedSearchInputModel.BeerTypeIds))]
    public async Task ValidateAsync_fails_when_an_id_list_has_more_than_25_items(string property)
    {
        var model = new FacetedSearchInputModel();
        typeof(FacetedSearchInputModel).GetProperty(property)!.SetValue(model, Ids(26));

        var result = await _sut.ValidateAsync(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Any(x => x.PropertyName == property).Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_passes_when_name_is_100_characters()
    {
        var result = await _sut.ValidateAsync(new FacetedSearchInputModel { Name = new string('a', 100) });

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_fails_when_name_is_longer_than_100_characters()
    {
        var result = await _sut.ValidateAsync(new FacetedSearchInputModel { Name = new string('a', 101) });

        result.IsValid.Should().BeFalse();
        result.Errors.Any(x => x.PropertyName == "Name").Should().BeTrue();
    }

    [Fact]
    public void PageSize_is_capped_at_50()
    {
        new DiscoverBeerParameter { PageSize = 500 }.PageSize.Should().Be(50);
    }
}

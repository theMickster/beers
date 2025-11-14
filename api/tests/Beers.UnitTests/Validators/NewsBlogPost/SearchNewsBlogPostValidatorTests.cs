using Beers.Application.Validators.NewsBlogPost;
using Beers.Common.Filtering.NewsBlogPost;

namespace Beers.UnitTests.Validators.NewsBlogPost;

public sealed class SearchNewsBlogPostValidatorTests
{
    private readonly SearchNewsBlogPostValidator _sut = new();

    [Fact]
    public async Task ValidateAsync_passes_when_date_range_start_is_before_end()
    {
        var model = new SearchInputNewsBlogPostModel
        {
            DateRangeStart = new DateTime(2025, 1, 1),
            DateRangeEnd = new DateTime(2025, 12, 31)
        };

        var result = await _sut.ValidateAsync(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_fails_when_date_range_start_is_after_end()
    {
        var model = new SearchInputNewsBlogPostModel
        {
            DateRangeStart = new DateTime(2025, 12, 31),
            DateRangeEnd = new DateTime(2025, 1, 1)
        };

        var result = await _sut.ValidateAsync(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Any(x => x.PropertyName == "DateRangeEnd").Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_passes_when_only_date_range_start_is_provided()
    {
        var model = new SearchInputNewsBlogPostModel
        {
            DateRangeStart = new DateTime(2025, 1, 1),
            DateRangeEnd = null
        };

        var result = await _sut.ValidateAsync(model);

        result.IsValid.Should().BeTrue();
    }
}

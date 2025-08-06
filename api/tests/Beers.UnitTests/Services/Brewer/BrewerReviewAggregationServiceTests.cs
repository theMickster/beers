using Beers.Application.Services.Brewer;

namespace Beers.UnitTests.Services.Brewer;

public sealed class BrewerReviewAggregationServiceTests
{
    [Fact]
    public void BuildAggregate_returns_zero_values_when_no_reviews()
    {
        var result = BrewerReviewAggregationService.BuildAggregate([]);

        using (new AssertionScope())
        {
            result.ReviewCount.Should().Be(0);
            result.Average.Should().Be(0);
        }
    }

    [Fact]
    public void BuildAggregate_returns_average_and_count_when_reviews_exist()
    {
        var result = BrewerReviewAggregationService.BuildAggregate([5, 4, 3, 4]);

        using (new AssertionScope())
        {
            result.ReviewCount.Should().Be(4);
            result.Average.Should().Be(4);
        }
    }
}

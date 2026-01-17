using Beers.Common.Constants;
using Beers.Common.Filtering.Beer;

namespace Beers.UnitTests.Filtering.Beer;

public sealed class DiscoverBeerParameterTests
{
    [Fact]
    public void OrderBy_defaults_to_name()
    {
        new DiscoverBeerParameter().OrderBy.Should().Be(SortedResultConstants.Name);
    }

    [Theory]
    [InlineData("Name", "Name")]
    [InlineData("ReviewCount", "ReviewCount")]
    [InlineData("CreatedDate", "CreatedDate")]
    [InlineData("reviewcount", "ReviewCount")]
    [InlineData("  CREATEDDATE ", "CreatedDate")]
    [InlineData("name", "Name")]
    public void OrderBy_accepts_known_values_case_insensitively(string input, string expected)
    {
        new DiscoverBeerParameter { OrderBy = input }.OrderBy.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("bogus")]
    [InlineData("Rating")]
    public void OrderBy_falls_back_to_name_for_unknown_values(string input)
    {
        new DiscoverBeerParameter { OrderBy = input }.OrderBy.Should().Be(SortedResultConstants.Name);
    }
}

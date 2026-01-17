using Beers.Application.Services.Beer;
using Beers.Common.Filtering.Beer;

namespace Beers.UnitTests.Services.Beer;

public sealed class DiscoverBeerQueryBuilderTests
{
    private static List<Guid> Ids(int count) => Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList();

    private static string Sql(FacetedSearchInputModel input, DiscoverFacet facet) =>
        DiscoverBeerQueryBuilder.Build(input, facet).QueryText;

    private static Dictionary<string, object> Parameters(FacetedSearchInputModel input, DiscoverFacet facet) =>
        DiscoverBeerQueryBuilder.Build(input, facet).GetQueryParameters().ToDictionary(x => x.Name, x => x.Value);

    [Theory]
    [InlineData(DiscoverFacet.Style, "JOIN s IN c.BeerStyles", "GROUP BY s.metadataId, s.name")]
    [InlineData(DiscoverFacet.Category, "JOIN cat IN c.BeerCategories", "GROUP BY cat.metadataId, cat.name")]
    [InlineData(DiscoverFacet.Brewer, null, "GROUP BY c.Brewer.id, c.Brewer.name")]
    [InlineData(DiscoverFacet.BeerType, null, "GROUP BY c.BeerType.metadataId, c.BeerType.name")]
    public void Build_with_no_filters_only_filters_on_entity_type(DiscoverFacet facet, string? join, string groupBy)
    {
        var query = DiscoverBeerQueryBuilder.Build(new FacetedSearchInputModel(), facet);

        query.QueryText.Should().Contain("WHERE c.EntityType = @entityType ");
        query.QueryText.Should().Contain("COUNT(1) AS BeerCount");
        query.QueryText.Should().EndWith(groupBy);
        query.QueryText.Should().NotContain("EXISTS").And.NotContain("c.BrewerId").And.NotContain("c.Rating");
        if (join == null)
        {
            query.QueryText.Should().NotContain("JOIN");
        }
        else
        {
            query.QueryText.Should().Contain(join);
        }

        query.GetQueryParameters().Should().ContainSingle().Which.Should().Be(("@entityType", "Beer"));
    }

    [Fact]
    public void Build_style_facet_excludes_its_own_filter_but_applies_the_others()
    {
        var input = new FacetedSearchInputModel { BeerStyleIds = Ids(2), BeerCategoryIds = Ids(2) };

        var sql = Sql(input, DiscoverFacet.Style);

        sql.Should().NotContain("x IN c.BeerStyles");
        sql.Should().Contain("EXISTS (SELECT VALUE x FROM x IN c.BeerCategories WHERE x.metadataId IN (@category0, @category1))");
        Parameters(input, DiscoverFacet.Style).Keys.Should().NotContain(k => k.StartsWith("@style"));
    }

    [Fact]
    public void Build_category_facet_excludes_its_own_filter_but_applies_the_others()
    {
        var input = new FacetedSearchInputModel { BeerStyleIds = Ids(2), BeerCategoryIds = Ids(2) };

        var sql = Sql(input, DiscoverFacet.Category);

        sql.Should().Contain("EXISTS (SELECT VALUE x FROM x IN c.BeerStyles WHERE x.metadataId IN (@style0, @style1))");
        sql.Should().NotContain("x IN c.BeerCategories");
    }

    [Fact]
    public void Build_brewer_facet_excludes_the_brewer_filter()
    {
        var input = new FacetedSearchInputModel { BrewerIds = Ids(2), BeerTypeIds = Ids(1) };

        var sql = Sql(input, DiscoverFacet.Brewer);

        sql.Should().NotContain("c.BrewerId");
        sql.Should().Contain("c.BeerType.metadataId IN (@type0)");
    }

    [Fact]
    public void Build_type_facet_excludes_the_type_filter()
    {
        var input = new FacetedSearchInputModel { BrewerIds = Ids(1), BeerTypeIds = Ids(2) };

        var sql = Sql(input, DiscoverFacet.BeerType);

        sql.Should().Contain("c.BrewerId = @brewer0");
        sql.Should().NotContain("c.BeerType.metadataId IN");
    }

    [Fact]
    public void Build_uses_equality_for_a_single_brewer_and_in_for_several()
    {
        Sql(new FacetedSearchInputModel { BrewerIds = Ids(1) }, DiscoverFacet.Style)
            .Should().Contain("c.BrewerId = @brewer0").And.NotContain("c.BrewerId IN");

        Sql(new FacetedSearchInputModel { BrewerIds = Ids(2) }, DiscoverFacet.Style)
            .Should().Contain("c.BrewerId IN (@brewer0, @brewer1)");
    }

    [Fact]
    public void Build_adds_inclusive_rating_bounds_as_parameters()
    {
        var input = new FacetedSearchInputModel { MinRating = 4m, MaxRating = 4.5m };

        var sql = Sql(input, DiscoverFacet.Style);
        var parameters = Parameters(input, DiscoverFacet.Style);

        sql.Should().Contain("c.Rating.average >= @minRating").And.Contain("c.Rating.average <= @maxRating");
        parameters["@minRating"].Should().Be(4m);
        parameters["@maxRating"].Should().Be(4.5m);
    }

    [Fact]
    public void Build_adds_a_case_insensitive_name_contains_and_trims_the_value()
    {
        var input = new FacetedSearchInputModel { Name = "  IPA " };

        Sql(input, DiscoverFacet.Brewer).Should().Contain("CONTAINS(c.Name, @name, true)");
        Parameters(input, DiscoverFacet.Brewer)["@name"].Should().Be("IPA");
    }

    [Fact]
    public void Build_ignores_a_whitespace_name()
    {
        Sql(new FacetedSearchInputModel { Name = "   " }, DiscoverFacet.Brewer).Should().NotContain("CONTAINS");
    }

    [Fact]
    public void Build_passes_ids_as_lowercase_string_parameters_and_never_inlines_values()
    {
        var id = Guid.NewGuid();
        var input = new FacetedSearchInputModel { BeerStyleIds = [id], Name = "'; DROP" };

        var sql = Sql(input, DiscoverFacet.Category);
        var parameters = Parameters(input, DiscoverFacet.Category);

        parameters["@style0"].Should().Be(id.ToString().ToLowerInvariant());
        sql.Should().NotContain(id.ToString()).And.NotContain("DROP");
    }

    [Fact]
    public void Build_creates_one_parameter_per_id_for_the_maximum_of_25_ids()
    {
        var input = new FacetedSearchInputModel { BeerStyleIds = Ids(25) };

        var parameters = Parameters(input, DiscoverFacet.Category);

        parameters.Keys.Count(k => k.StartsWith("@style")).Should().Be(25);
        parameters.Should().ContainKey("@style24");
    }

    [Fact]
    public void Build_throws_for_a_null_input()
    {
        var act = () => DiscoverBeerQueryBuilder.Build(null!, DiscoverFacet.Style);

        act.Should().Throw<ArgumentNullException>();
    }
}

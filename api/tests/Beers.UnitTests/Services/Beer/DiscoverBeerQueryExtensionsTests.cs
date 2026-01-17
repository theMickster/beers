using Beers.Application.Data;
using Beers.Application.Services.Beer;
using Beers.Common.Filtering.Beer;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Beers.Domain.Entities.Slims;
using Microsoft.EntityFrameworkCore;

namespace Beers.UnitTests.Services.Beer;

public sealed class DiscoverBeerQueryExtensionsTests
{
    private static readonly Guid StyleIpa = Guid.NewGuid();
    private static readonly Guid StyleLager = Guid.NewGuid();
    private static readonly Guid StyleStout = Guid.NewGuid();
    private static readonly Guid CategoryDomestic = Guid.NewGuid();
    private static readonly Guid CategoryImport = Guid.NewGuid();
    private static readonly Guid BrewerA = Guid.NewGuid();
    private static readonly Guid BrewerB = Guid.NewGuid();
    private static readonly Guid BrewerC = Guid.NewGuid();
    private static readonly Guid TypeAle = Guid.NewGuid();
    private static readonly Guid TypeLager = Guid.NewGuid();

    private static BeerEntity Beer(string name, Guid brewerId, Guid typeId, decimal average, int reviews, DateTime created,
        Guid[] styles, Guid[] categories) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            BrewerId = brewerId,
            BeerType = new BeerTypeSlimEntity { MetadataId = typeId },
            Rating = new RatingEntity { Average = average, ReviewCount = reviews },
            CreatedDate = created,
            BeerStyles = styles.Select(x => new BeerStyleSlimEntity { MetadataId = x }).ToList(),
            BeerCategories = categories.Select(x => new BeerCategorySlimEntity { MetadataId = x }).ToList()
        };

    private static readonly List<BeerEntity> Beers =
    [
        Beer("Alpha IPA", BrewerA, TypeAle, 4.5m, 10, new DateTime(2024, 1, 1), [StyleIpa], [CategoryDomestic]),
        Beer("Bravo Lager", BrewerB, TypeLager, 4.0m, 30, new DateTime(2024, 3, 1), [StyleLager], [CategoryImport]),
        Beer("Charlie Stout", BrewerC, TypeAle, 3.0m, 30, new DateTime(2024, 2, 1), [StyleStout], [CategoryDomestic, CategoryImport]),
        Beer("Delta Hazy IPA", BrewerA, TypeAle, 2.0m, 5, new DateTime(2024, 4, 1), [StyleIpa, StyleStout], [CategoryDomestic]),
        Beer("Echo Unrated", BrewerB, TypeLager, 0m, 0, new DateTime(2024, 5, 1), [StyleLager], [CategoryImport])
    ];

    private static List<string> Names(FacetedSearchInputModel input) =>
        Beers.AsQueryable().ApplyFilters(input).Select(x => x.Name).OrderBy(x => x).ToList();

    private static List<string> Sorted(string orderBy, string sortOrder) =>
        Beers.AsQueryable()
            .ApplySort(new DiscoverBeerParameter { OrderBy = orderBy, SortOrder = sortOrder })
            .Select(x => x.Name).ToList();

    [Fact]
    public void ApplyFilters_with_no_filters_returns_everything()
    {
        Names(new FacetedSearchInputModel()).Should().HaveCount(5);
    }

    [Fact]
    public void ApplyFilters_ors_values_within_a_style_filter()
    {
        Names(new FacetedSearchInputModel { BeerStyleIds = [StyleIpa, StyleLager] })
            .Should().Equal("Alpha IPA", "Bravo Lager", "Delta Hazy IPA", "Echo Unrated");
    }

    [Fact]
    public void ApplyFilters_matches_a_beer_having_any_of_several_styles()
    {
        Names(new FacetedSearchInputModel { BeerStyleIds = [StyleStout] })
            .Should().Equal("Charlie Stout", "Delta Hazy IPA");
    }

    [Fact]
    public void ApplyFilters_ors_values_within_a_category_filter()
    {
        Names(new FacetedSearchInputModel { BeerCategoryIds = [CategoryDomestic] })
            .Should().Equal("Alpha IPA", "Charlie Stout", "Delta Hazy IPA");
    }

    [Fact]
    public void ApplyFilters_ands_different_filters_together()
    {
        Names(new FacetedSearchInputModel { BeerStyleIds = [StyleIpa], BeerCategoryIds = [CategoryDomestic], MinRating = 4m })
            .Should().Equal("Alpha IPA");
    }

    [Fact]
    public void ApplyFilters_filters_by_a_single_brewer()
    {
        Names(new FacetedSearchInputModel { BrewerIds = [BrewerA] }).Should().Equal("Alpha IPA", "Delta Hazy IPA");
    }

    [Fact]
    public void ApplyFilters_filters_by_several_brewers()
    {
        Names(new FacetedSearchInputModel { BrewerIds = [BrewerB, BrewerC] })
            .Should().Equal("Bravo Lager", "Charlie Stout", "Echo Unrated");
    }

    [Fact]
    public void ApplyFilters_filters_by_beer_type()
    {
        Names(new FacetedSearchInputModel { BeerTypeIds = [TypeLager] }).Should().Equal("Bravo Lager", "Echo Unrated");
    }

    [Fact]
    public void ApplyFilters_treats_rating_bounds_as_inclusive()
    {
        Names(new FacetedSearchInputModel { MinRating = 3m, MaxRating = 4m }).Should().Equal("Bravo Lager", "Charlie Stout");
    }

    [Fact]
    public void ApplyFilters_excludes_unrated_beers_when_min_rating_is_greater_than_zero()
    {
        Names(new FacetedSearchInputModel { MinRating = 0.5m }).Should().NotContain("Echo Unrated");
    }

    [Fact]
    public void ApplyFilters_matches_name_case_insensitively()
    {
        Names(new FacetedSearchInputModel { Name = "ipa" }).Should().Equal("Alpha IPA", "Delta Hazy IPA");
    }

    [Fact]
    public void ApplyFilters_ignores_a_blank_name()
    {
        Names(new FacetedSearchInputModel { Name = "  " }).Should().HaveCount(5);
    }

    [Fact]
    public void ApplySort_orders_by_name_ascending_and_descending()
    {
        Sorted("Name", "Ascending").Should().Equal("Alpha IPA", "Bravo Lager", "Charlie Stout", "Delta Hazy IPA", "Echo Unrated");
        Sorted("Name", "Descending").Should().Equal("Echo Unrated", "Delta Hazy IPA", "Charlie Stout", "Bravo Lager", "Alpha IPA");
    }

    [Fact]
    public void ApplySort_orders_by_review_count_with_a_name_tiebreak_in_the_same_direction()
    {
        Sorted("ReviewCount", "Ascending")
            .Should().Equal("Echo Unrated", "Delta Hazy IPA", "Alpha IPA", "Bravo Lager", "Charlie Stout");
        Sorted("ReviewCount", "Descending")
            .Should().Equal("Charlie Stout", "Bravo Lager", "Alpha IPA", "Delta Hazy IPA", "Echo Unrated");
    }

    [Fact]
    public void ApplySort_orders_by_created_date()
    {
        Sorted("CreatedDate", "Ascending")
            .Should().Equal("Alpha IPA", "Charlie Stout", "Bravo Lager", "Delta Hazy IPA", "Echo Unrated");
        Sorted("CreatedDate", "Descending")
            .Should().Equal("Echo Unrated", "Delta Hazy IPA", "Bravo Lager", "Charlie Stout", "Alpha IPA");
    }

    // No network is used: ToQueryString only compiles the query against a Cosmos-configured model.
    [Fact]
    public void Filters_and_sort_translate_to_cosmos_sql()
    {
        var options = new DbContextOptionsBuilder<BeersDbContext>()
            .UseCosmos("https://localhost:8081/", Convert.ToBase64String(new byte[64]), "unit-test")
            .Options;
        using var context = new BeersDbContext(options);
        var input = new FacetedSearchInputModel
        {
            BeerStyleIds = [StyleIpa, StyleLager], BeerCategoryIds = [CategoryDomestic], BrewerIds = [BrewerA, BrewerB],
            BeerTypeIds = [TypeAle], MinRating = 4m, MaxRating = 5m, Name = "ipa"
        };

        var sql = context.BeerEntities.ApplyFilters(input)
            .ApplySort(new DiscoverBeerParameter { OrderBy = "ReviewCount", SortOrder = "desc" })
            .Skip(10).Take(10)
            .ToQueryString();

        sql.Should().Contain("EXISTS").And.Contain("c[\"BeerStyles\"]").And.Contain("c[\"BeerCategories\"]");
        sql.Should().Contain("CONTAINS(c[\"Name\"], @name, true)");
        sql.Should().Contain("ORDER BY c[\"Rating\"][\"reviews\"] DESC, c[\"Name\"] DESC");
        sql.Should().Contain("OFFSET").And.Contain("LIMIT");
    }
}

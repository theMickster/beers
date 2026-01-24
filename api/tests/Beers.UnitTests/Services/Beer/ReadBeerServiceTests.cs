using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Services.Beer;
using Beers.Common.Constants;
using Beers.Common.Filtering.Beer;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Beers.Domain.Entities.Slims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;

namespace Beers.UnitTests.Services.Beer;

public sealed class ReadBeerServiceTests
{
    private static readonly Guid StyleIpa = Guid.NewGuid();
    private static readonly Guid StyleLager = Guid.NewGuid();
    private static readonly Guid CategoryDomestic = Guid.NewGuid();
    private static readonly Guid CategoryImport = Guid.NewGuid();
    private static readonly Guid TypeAle = Guid.NewGuid();
    private static readonly Guid TypeLager = Guid.NewGuid();
    private static readonly Guid BrewerA = Guid.NewGuid();
    private static readonly Guid BrewerB = Guid.NewGuid();

    private readonly IMapper _mapper;

    public ReadBeerServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<Beers.Domain.Profiles.BeerEntityToModelProfile>();
        }, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    private static BeerEntity Beer(string name, Guid brewerId, Guid typeId, Guid style, Guid category, params decimal[] prices) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            BrewerId = brewerId,
            Brewer = new BrewerSlimEntity { Name = brewerId == BrewerA ? "Alpha Brewing" : "Beta Brewing" },
            BeerType = new BeerTypeSlimEntity { MetadataId = typeId },
            BeerStyles = [new BeerStyleSlimEntity { MetadataId = style }],
            BeerCategories = [new BeerCategorySlimEntity { MetadataId = category }],
            Pricing = prices.Select(x => new PriceEntity { Price = x }).ToList()
        };

    // Names are chosen so the default name sort gives Alpha, Bravo, Charlie, Delta.
    private static List<BeerEntity> Beers() =>
    [
        Beer("Alpha", BrewerA, TypeAle, StyleIpa, CategoryDomestic, 5m),
        Beer("Bravo", BrewerB, TypeLager, StyleLager, CategoryImport, 12m),
        Beer("Charlie", BrewerA, TypeAle, StyleIpa, CategoryImport, 20m),
        Beer("Delta", BrewerB, TypeLager, StyleIpa, CategoryDomestic, 8m, 30m)
    ];

    private ReadBeerService BuildSut(List<BeerEntity> entities)
    {
        var mockDbSet = entities.BuildMockDbSet<BeerEntity>();
        var dbFactory = new Mock<IDbContextFactory<BeersDbContext>>();
        dbFactory.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new FakeBeersDbContext(new DbContextOptionsBuilder<BeersDbContext>().Options)
            {
                BeerEntities = mockDbSet.Object
            });

        return new ReadBeerService(_mapper, dbFactory.Object);
    }

    private async Task<List<string>> Search(SearchInputBeerModel input)
    {
        var result = await BuildSut(Beers()).SearchAsync(new SearchBeerParameter(), input);
        result.TotalRecords.Should().Be(result.Results.Count);
        return result.Results.Select(x => x.Name).ToList();
    }

    [Fact]
    public async Task SearchAsync_with_an_empty_body_returns_every_beer()
    {
        (await Search(new SearchInputBeerModel())).Should().Equal("Alpha", "Bravo", "Charlie", "Delta");
    }

    [Fact]
    public async Task SearchAsync_filters_by_one_style()
    {
        (await Search(new SearchInputBeerModel { BeerStyleIds = [StyleLager] })).Should().Equal("Bravo");
    }

    [Fact]
    public async Task SearchAsync_ORs_values_within_the_style_list()
    {
        (await Search(new SearchInputBeerModel { BeerStyleIds = [StyleLager, StyleIpa] }))
            .Should().Equal("Alpha", "Bravo", "Charlie", "Delta");
    }

    [Fact]
    public async Task SearchAsync_ANDs_style_with_category()
    {
        (await Search(new SearchInputBeerModel { BeerStyleIds = [StyleIpa], BeerCategoryIds = [CategoryImport] }))
            .Should().Equal("Charlie");
    }

    [Fact]
    public async Task SearchAsync_filters_by_type()
    {
        (await Search(new SearchInputBeerModel { BeerTypeIds = [TypeLager] })).Should().Equal("Bravo", "Delta");
    }

    [Fact]
    public async Task SearchAsync_filters_by_min_price_only()
    {
        (await Search(new SearchInputBeerModel { MinPrice = 20m })).Should().Equal("Charlie", "Delta");
    }

    [Fact]
    public async Task SearchAsync_filters_by_max_price_only()
    {
        (await Search(new SearchInputBeerModel { MaxPrice = 8m })).Should().Equal("Alpha", "Delta");
    }

    [Fact]
    public async Task SearchAsync_filters_by_a_price_range()
    {
        (await Search(new SearchInputBeerModel { MinPrice = 10m, MaxPrice = 15m })).Should().Equal("Bravo");
    }

    [Fact]
    public async Task SearchAsync_combines_the_new_filters_with_the_existing_ones()
    {
        var input = new SearchInputBeerModel
        {
            BrewerId = BrewerB,
            BeerTypeIds = [TypeLager],
            BeerStyleIds = [StyleIpa],
            MaxPrice = 10m,
            Name = "Delta"
        };

        (await Search(input)).Should().Equal("Delta");
        (await Search(new SearchInputBeerModel { BrewerId = BrewerA, BeerTypeIds = [TypeLager] })).Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_still_matches_the_name_case_sensitively()
    {
        (await Search(new SearchInputBeerModel { Name = "alpha" })).Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_filters_by_brewer_name_with_a_style()
    {
        (await Search(new SearchInputBeerModel { BrewerName = "Beta", BeerStyleIds = [StyleIpa] })).Should().Equal("Delta");
    }

    [Fact]
    public async Task SearchAsync_treats_null_id_lists_as_empty()
    {
        var input = new SearchInputBeerModel { BeerStyleIds = null!, BeerCategoryIds = null!, BeerTypeIds = null! };

        (await Search(input)).Should().HaveCount(4);
    }

    [Fact]
    public async Task SearchAsync_counts_all_matches_but_returns_only_the_requested_page()
    {
        var parameters = new SearchBeerParameter { PageNumber = 2, PageSize = 1, SortOrder = SortedResultConstants.Ascending };

        var result = await BuildSut(Beers()).SearchAsync(parameters, new SearchInputBeerModel { BeerStyleIds = [StyleIpa] });

        result.TotalRecords.Should().Be(3);
        result.Results.Select(x => x.Name).Should().Equal("Charlie");
    }

    private sealed class FakeBeersDbContext(DbContextOptions<BeersDbContext> options) : BeersDbContext(options)
    {
    }
}

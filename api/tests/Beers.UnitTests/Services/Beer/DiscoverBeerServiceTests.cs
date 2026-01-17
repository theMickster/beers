using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Services.Beer;
using Beers.Application.Validators.Beer;
using Beers.Common.Filtering.Beer;
using Beers.Common.Settings;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Slims;
using FluentValidation;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;

namespace Beers.UnitTests.Services.Beer;

public sealed class DiscoverBeerServiceTests
{
    private readonly IMapper _mapper;
    private readonly Mock<CosmosClient> _cosmosClient = new();
    private readonly Mock<Container> _container = new();
    private readonly CosmosDbConnectionSettings _settings = new() { DatabaseName = "unit-test-db" };

    public DiscoverBeerServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<Beers.Domain.Profiles.BeerEntityToModelProfile>();
        }, NullLoggerFactory.Instance);

        _mapper = config.CreateMapper();
        _cosmosClient.Setup(x => x.GetContainer(It.IsAny<string>(), It.IsAny<string>())).Returns(_container.Object);
    }

    private DiscoverBeerService BuildSut(List<BeerEntity> entities, IValidator<FacetedSearchInputModel>? validator = null)
    {
        var mockDbSet = entities.BuildMockDbSet<BeerEntity>();
        var dbFactory = new Mock<IDbContextFactory<BeersDbContext>>();
        dbFactory.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new FakeBeersDbContext(new DbContextOptionsBuilder<BeersDbContext>().Options)
            {
                BeerEntities = mockDbSet.Object
            });

        return new DiscoverBeerService(_mapper, dbFactory.Object, validator ?? new FacetedSearchInputModelValidator(),
            _cosmosClient.Object, _settings, NullLogger<DiscoverBeerService>.Instance);
    }

    private void SetupFacetRows(Func<QueryDefinition, List<FacetRow>> rowsFor)
    {
        _container
            .Setup(x => x.GetItemQueryIterator<FacetRow>(It.IsAny<QueryDefinition>(), It.IsAny<string>(), It.IsAny<QueryRequestOptions>()))
            .Returns((QueryDefinition query, string? _, QueryRequestOptions? _) =>
            {
                var response = new Mock<FeedResponse<FacetRow>>();
                response.Setup(x => x.GetEnumerator()).Returns(() => rowsFor(query).GetEnumerator());
                response.Setup(x => x.RequestCharge).Returns(4.2);

                var iterator = new Mock<FeedIterator<FacetRow>>();
                iterator.SetupSequence(x => x.HasMoreResults).Returns(true).Returns(false);
                iterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(response.Object);
                return iterator.Object;
            });
    }

    [Fact]
    public async Task DiscoverAsync_throws_when_parameters_is_null()
    {
        var sut = BuildSut([]);

        var act = () => sut.DiscoverAsync(null!, new FacetedSearchInputModel());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DiscoverAsync_throws_when_input_is_null()
    {
        var sut = BuildSut([]);

        var act = () => sut.DiscoverAsync(new DiscoverBeerParameter(), null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DiscoverAsync_returns_failures_without_calling_cosmos_when_input_is_invalid()
    {
        var sut = BuildSut([]);

        var (result, errors) = await sut.DiscoverAsync(new DiscoverBeerParameter(),
            new FacetedSearchInputModel { MinRating = 5m, MaxRating = 1m });

        errors.Should().NotBeEmpty();
        result.Results.Should().BeEmpty();
        result.TotalRecords.Should().Be(0);
        _cosmosClient.Verify(x => x.GetContainer(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void ToFacetItems_orders_by_count_descending_then_name_ignoring_case_and_maps_the_count()
    {
        var ids = Enumerable.Range(0, 4).Select(_ => Guid.NewGuid()).ToList();
        var rows = new List<FacetRow>
        {
            new() { Id = ids[0], Name = "banana", BeerCount = 2 },
            new() { Id = ids[1], Name = "Apple", BeerCount = 2 },
            new() { Id = ids[2], Name = "Cherry", BeerCount = 9 },
            new() { Id = ids[3], Name = "Date", BeerCount = 1 }
        };

        var items = DiscoverBeerService.ToFacetItems(rows);

        items.Select(x => x.Name).Should().Equal("Cherry", "Apple", "banana", "Date");
        items.Select(x => x.Count).Should().Equal(9, 2, 2, 1);
        items[0].Id.Should().Be(ids[2]);
    }

    [Fact]
    public void ToFacetItems_returns_an_empty_list_for_no_rows()
    {
        DiscoverBeerService.ToFacetItems([]).Should().BeEmpty();
    }

    [Fact]
    public async Task DiscoverAsync_returns_the_page_total_and_facets_from_all_six_queries()
    {
        var entities = new List<BeerEntity>
        {
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Name = "Beta", Rating = new RatingEntity { Average = 4m } },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Name = "Alpha", Rating = new RatingEntity { Average = 4m } },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Name = "Gamma", Rating = new RatingEntity { Average = 2m } }
        };
        var queries = new List<string>();
        SetupFacetRows(query =>
        {
            lock (queries) { queries.Add(query.QueryText); }
            return [new FacetRow { Id = Guid.NewGuid(), Name = "Row", BeerCount = 3 }];
        });
        var sut = BuildSut(entities);

        var (result, errors) = await sut.DiscoverAsync(new DiscoverBeerParameter { PageSize = 1, PageNumber = 1 },
            new FacetedSearchInputModel { MinRating = 3m });

        errors.Should().BeEmpty();
        result.TotalRecords.Should().Be(2);
        result.Results.Select(x => x.Name).Should().Equal("Alpha");
        result.PageSize.Should().Be(1);
        result.Facets.StyleFacets.Should().ContainSingle().Which.Count.Should().Be(3);
        result.Facets.CategoryFacets.Should().ContainSingle();
        result.Facets.BrewerFacets.Should().ContainSingle();
        result.Facets.BeerTypeFacets.Should().ContainSingle();
        queries.Should().HaveCount(4);
        _cosmosClient.Verify(x => x.GetContainer("unit-test-db", "Beers"), Times.Once);
    }

    private sealed class FakeBeersDbContext(DbContextOptions<BeersDbContext> options) : BeersDbContext(options)
    {
    }
}

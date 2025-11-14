using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Services.NewsBlogPost;
using Beers.Common.Filtering.NewsBlogPost;
using Beers.Domain.Entities;
using Beers.Domain.Enums;
using Beers.Domain.Models.NewsBlogPost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;

namespace Beers.UnitTests.Services.NewsBlogPost;

public sealed class SearchNewsBlogPostServiceTests
{
    private readonly IMapper _mapper;

    public SearchNewsBlogPostServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<Beers.Domain.Profiles.NewsBlogPostEntityToModelProfile>();
        }, NullLoggerFactory.Instance);

        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    private static ReadNewsBlogPostService BuildSut(IMapper mapper, List<NewsBlogPostEntity> entities)
    {
        var mockDbSet = entities.BuildMockDbSet<NewsBlogPostEntity>();
        var context = new FakeBeersDbContext(new DbContextOptionsBuilder<BeersDbContext>().Options)
        {
            NewsBlogPostEntities = mockDbSet.Object
        };
        var dbFactory = new Mock<IDbContextFactory<BeersDbContext>>();
        dbFactory.Setup(x => x.CreateDbContextAsync(default)).ReturnsAsync(context);
        return new ReadNewsBlogPostService(mapper, dbFactory.Object);
    }

    [Fact]
    public async Task SearchAsync_returns_empty_result_when_parameters_is_null()
    {
        var sut = BuildSut(_mapper, []);
        var result = await sut.SearchAsync(null!, new SearchInputNewsBlogPostModel());
        result.Results.Should().BeEmpty();
        result.TotalRecords.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_returns_empty_result_when_search_model_is_null()
    {
        var sut = BuildSut(_mapper, []);
        var result = await sut.SearchAsync(new SearchNewsBlogPostParameter(), null!);
        result.Results.Should().BeEmpty();
        result.TotalRecords.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_excludes_drafts_by_default()
    {
        var entities = new List<NewsBlogPostEntity>
        {
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Published 1", PublishedDate = new DateTime(2024, 1, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Published 2", PublishedDate = new DateTime(2024, 2, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Draft", PublishedDate = null }
        };

        var sut = BuildSut(_mapper, entities);
        var result = await sut.SearchAsync(
            new SearchNewsBlogPostParameter(),
            new SearchInputNewsBlogPostModel { IncludeDrafts = false });

        result.Results.Count.Should().Be(2);
        result.TotalRecords.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_includes_drafts_when_flag_is_true()
    {
        var entities = new List<NewsBlogPostEntity>
        {
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Published 1", PublishedDate = new DateTime(2024, 1, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Published 2", PublishedDate = new DateTime(2024, 2, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Draft", PublishedDate = null }
        };

        var sut = BuildSut(_mapper, entities);
        var result = await sut.SearchAsync(
            new SearchNewsBlogPostParameter(),
            new SearchInputNewsBlogPostModel { IncludeDrafts = true });

        result.Results.Count.Should().Be(3);
        result.TotalRecords.Should().Be(3);
    }

    [Fact]
    public async Task SearchAsync_filters_by_brewer_id()
    {
        var brewerA = Guid.NewGuid();
        var brewerB = Guid.NewGuid();

        var entities = new List<NewsBlogPostEntity>
        {
            new() { Id = Guid.NewGuid(), BrewerId = brewerA, Title = "A1", PublishedDate = new DateTime(2024, 1, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = brewerA, Title = "A2", PublishedDate = new DateTime(2024, 2, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = brewerB, Title = "B1", PublishedDate = new DateTime(2024, 3, 1) }
        };

        var sut = BuildSut(_mapper, entities);
        var result = await sut.SearchAsync(
            new SearchNewsBlogPostParameter(),
            new SearchInputNewsBlogPostModel { BrewerId = brewerA, IncludeDrafts = true });

        result.Results.Count.Should().Be(2);
        result.TotalRecords.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_filters_by_post_type()
    {
        var entities = new List<NewsBlogPostEntity>
        {
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Text Post", PostType = NewsBlogPostType.TextPost, PublishedDate = new DateTime(2024, 1, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Gallery", PostType = NewsBlogPostType.ImageGallery, PublishedDate = new DateTime(2024, 2, 1) }
        };

        var sut = BuildSut(_mapper, entities);
        var result = await sut.SearchAsync(
            new SearchNewsBlogPostParameter(),
            new SearchInputNewsBlogPostModel { PostType = "ImageGallery", IncludeDrafts = true });

        result.Results.Count.Should().Be(1);
        result.TotalRecords.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_filters_by_tag()
    {
        var entities = new List<NewsBlogPostEntity>
        {
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "IPA Post", Tags = ["craft", "ipa"], PublishedDate = new DateTime(2024, 1, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Lager Post", Tags = ["lager"], PublishedDate = new DateTime(2024, 2, 1) }
        };

        var sut = BuildSut(_mapper, entities);
        var result = await sut.SearchAsync(
            new SearchNewsBlogPostParameter(),
            new SearchInputNewsBlogPostModel { Tag = "ipa", IncludeDrafts = true });

        result.Results.Count.Should().Be(1);
        result.TotalRecords.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_filters_by_date_range_start()
    {
        var feb1 = new DateTime(2024, 2, 1);

        var entities = new List<NewsBlogPostEntity>
        {
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Jan", PublishedDate = new DateTime(2024, 1, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Feb", PublishedDate = feb1 },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Mar", PublishedDate = new DateTime(2024, 3, 1) }
        };

        var sut = BuildSut(_mapper, entities);
        var result = await sut.SearchAsync(
            new SearchNewsBlogPostParameter(),
            new SearchInputNewsBlogPostModel { DateRangeStart = feb1, IncludeDrafts = true });

        result.Results.Count.Should().Be(2);
        result.TotalRecords.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_filters_by_date_range_end()
    {
        var feb1 = new DateTime(2024, 2, 1);

        var entities = new List<NewsBlogPostEntity>
        {
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Jan", PublishedDate = new DateTime(2024, 1, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Feb", PublishedDate = feb1 },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Mar", PublishedDate = new DateTime(2024, 3, 1) }
        };

        var sut = BuildSut(_mapper, entities);
        var result = await sut.SearchAsync(
            new SearchNewsBlogPostParameter(),
            new SearchInputNewsBlogPostModel { DateRangeEnd = feb1, IncludeDrafts = true });

        result.Results.Count.Should().Be(2);
        result.TotalRecords.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_filters_by_date_range_both_bounds()
    {
        var feb1 = new DateTime(2024, 2, 1);

        var entities = new List<NewsBlogPostEntity>
        {
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Jan", PublishedDate = new DateTime(2024, 1, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Feb", PublishedDate = feb1 },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Mar", PublishedDate = new DateTime(2024, 3, 1) }
        };

        var sut = BuildSut(_mapper, entities);
        var result = await sut.SearchAsync(
            new SearchNewsBlogPostParameter(),
            new SearchInputNewsBlogPostModel { DateRangeStart = feb1, DateRangeEnd = feb1, IncludeDrafts = true });

        result.Results.Count.Should().Be(1);
        result.TotalRecords.Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_paginates_correctly()
    {
        var entities = Enumerable.Range(1, 5)
            .Select(i => new NewsBlogPostEntity
            {
                Id = Guid.NewGuid(),
                BrewerId = Guid.NewGuid(),
                Title = $"Post {i}",
                PublishedDate = new DateTime(2024, i, 1)
            })
            .ToList();

        var sut = BuildSut(_mapper, entities);
        var result = await sut.SearchAsync(
            new SearchNewsBlogPostParameter { PageSize = 2, PageNumber = 2 },
            new SearchInputNewsBlogPostModel { IncludeDrafts = true });

        result.Results.Count.Should().Be(2);
        result.TotalRecords.Should().Be(5);
    }

    [Fact]
    public async Task SearchAsync_orders_by_title_ascending()
    {
        var entities = new List<NewsBlogPostEntity>
        {
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Zymurgy Notes", PublishedDate = new DateTime(2024, 1, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Barrel Aged", PublishedDate = new DateTime(2024, 2, 1) },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Craft Corner", PublishedDate = new DateTime(2024, 3, 1) }
        };

        var sut = BuildSut(_mapper, entities);
        var result = await sut.SearchAsync(
            new SearchNewsBlogPostParameter { OrderBy = "title", SortOrder = "ascending" },
            new SearchInputNewsBlogPostModel { IncludeDrafts = true });

        result.Results[0].Title.Should().Be("Barrel Aged");
    }

    [Fact]
    public async Task SearchAsync_orders_by_published_date_descending()
    {
        var jan = new DateTime(2024, 1, 1);
        var feb = new DateTime(2024, 2, 1);
        var mar = new DateTime(2024, 3, 1);

        var entities = new List<NewsBlogPostEntity>
        {
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Jan Post", PublishedDate = jan },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Feb Post", PublishedDate = feb },
            new() { Id = Guid.NewGuid(), BrewerId = Guid.NewGuid(), Title = "Mar Post", PublishedDate = mar }
        };

        var sut = BuildSut(_mapper, entities);
        var result = await sut.SearchAsync(
            new SearchNewsBlogPostParameter { OrderBy = "publishedDate", SortOrder = "descending" },
            new SearchInputNewsBlogPostModel { IncludeDrafts = true });

        result.Results[0].PublishedDate.Should().Be(mar);
    }

    private sealed class FakeBeersDbContext(DbContextOptions<BeersDbContext> options) : BeersDbContext(options)
    {
    }
}

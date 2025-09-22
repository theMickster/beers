using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Services.NewsBlogPost;
using Beers.Domain.Entities;
using Beers.Domain.Models.NewsBlogPost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;

namespace Beers.UnitTests.Services.NewsBlogPost;

public sealed class ReadNewsBlogPostServiceTests
{
    private readonly IMapper _mapper;

    public ReadNewsBlogPostServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<Beers.Domain.Profiles.NewsBlogPostEntityToModelProfile>();
        }, NullLoggerFactory.Instance);

        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task GetListAsync_filters_and_orders_by_published_date_descending()
    {
        var brewerId = Guid.NewGuid();
        var otherBrewerId = Guid.NewGuid();

        var oldest = new NewsBlogPostEntity { Id = Guid.NewGuid(), BrewerId = brewerId, PublishedDate = new DateTime(2024, 1, 1), Title = "Oldest" };
        var newest = new NewsBlogPostEntity { Id = Guid.NewGuid(), BrewerId = brewerId, PublishedDate = new DateTime(2024, 3, 1), Title = "Newest" };
        var middle = new NewsBlogPostEntity { Id = Guid.NewGuid(), BrewerId = brewerId, PublishedDate = new DateTime(2024, 2, 1), Title = "Middle" };
        var ignored = new NewsBlogPostEntity { Id = Guid.NewGuid(), BrewerId = otherBrewerId, PublishedDate = new DateTime(2024, 4, 1), Title = "Ignored" };

        var list = new List<NewsBlogPostEntity> { oldest, newest, middle, ignored };
        var mockDbSet = list.BuildMockDbSet<NewsBlogPostEntity>();

        await using var context = new FakeBeersDbContext(new DbContextOptionsBuilder<BeersDbContext>().Options)
        {
            NewsBlogPostEntities = mockDbSet.Object
        };

        var dbFactory = new Mock<IDbContextFactory<BeersDbContext>>();
        dbFactory.Setup(x => x.CreateDbContextAsync(default)).ReturnsAsync(context);
        var sut = new ReadNewsBlogPostService(_mapper, dbFactory.Object);

        var result = await sut.GetListAsync(brewerId);

        result.Select(x => x.Title).Should().Equal("Newest", "Middle", "Oldest");
        result.Should().OnlyContain(x => x.BrewerId == brewerId);
    }

    [Fact]
    public async Task GetListAsync_returns_empty_list_when_none_exist()
    {
        var brewerId = Guid.NewGuid();
        var mockDbSet = new List<NewsBlogPostEntity>().BuildMockDbSet<NewsBlogPostEntity>();

        await using var context = new FakeBeersDbContext(new DbContextOptionsBuilder<BeersDbContext>().Options)
        {
            NewsBlogPostEntities = mockDbSet.Object
        };

        var dbFactory = new Mock<IDbContextFactory<BeersDbContext>>();
        dbFactory.Setup(x => x.CreateDbContextAsync(default)).ReturnsAsync(context);
        var sut = new ReadNewsBlogPostService(_mapper, dbFactory.Object);

        var result = await sut.GetListAsync(brewerId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_returns_model_when_brewer_and_post_match()
    {
        var brewerId = Guid.NewGuid();
        var post = new NewsBlogPostEntity
        {
            Id = Guid.NewGuid(),
            BrewerId = brewerId,
            PublishedDate = DateTime.UtcNow,
            Title = "Title",
            Body = "Body"
        };

        var mockDbSet = new List<NewsBlogPostEntity> { post }.BuildMockDbSet<NewsBlogPostEntity>();

        await using var context = new FakeBeersDbContext(new DbContextOptionsBuilder<BeersDbContext>().Options)
        {
            NewsBlogPostEntities = mockDbSet.Object
        };

        var dbFactory = new Mock<IDbContextFactory<BeersDbContext>>();
        dbFactory.Setup(x => x.CreateDbContextAsync(default)).ReturnsAsync(context);
        var sut = new ReadNewsBlogPostService(_mapper, dbFactory.Object);

        var result = await sut.GetByIdAsync(brewerId, post.Id);

        result.Should().NotBeNull();
        result!.NewsBlogPostId.Should().Be(post.Id);
        result.BrewerId.Should().Be(brewerId);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_brewer_does_not_match()
    {
        var post = new NewsBlogPostEntity
        {
            Id = Guid.NewGuid(),
            BrewerId = Guid.NewGuid(),
            PublishedDate = DateTime.UtcNow,
            Title = "Title"
        };

        var mockDbSet = new List<NewsBlogPostEntity> { post }.BuildMockDbSet<NewsBlogPostEntity>();

        await using var context = new FakeBeersDbContext(new DbContextOptionsBuilder<BeersDbContext>().Options)
        {
            NewsBlogPostEntities = mockDbSet.Object
        };

        var dbFactory = new Mock<IDbContextFactory<BeersDbContext>>();
        dbFactory.Setup(x => x.CreateDbContextAsync(default)).ReturnsAsync(context);
        var sut = new ReadNewsBlogPostService(_mapper, dbFactory.Object);

        var result = await sut.GetByIdAsync(Guid.NewGuid(), post.Id);

        result.Should().BeNull();
    }

    private sealed class FakeBeersDbContext(DbContextOptions<BeersDbContext> options) : BeersDbContext(options)
    {
    }
}

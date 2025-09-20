using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Hydration;
using Beers.Application.Services.NewsBlogPost;
using Beers.Domain.Entities;
using Beers.Domain.Models.NewsBlogPost;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;

namespace Beers.UnitTests.Services.NewsBlogPost;

public sealed class CreateNewsBlogPostServiceTests
{
    private readonly Mock<IValidator<CreateNewsBlogPostModel>> _mockValidator = new();
    private readonly Mock<INewsBlogPostHydrationService> _mockHydration = new();
    private readonly Mock<IDbContextFactory<BeersDbContext>> _mockDbFactory = new();
    private readonly IMapper _mapper;

    public CreateNewsBlogPostServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<Beers.Domain.Profiles.NewsBlogPostEntityToModelProfile>();
        }, NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task CreateAsync_returns_model_when_valid()
    {
        // arrange
        var model = new CreateNewsBlogPostModel { BrewerId = Guid.NewGuid(), Title = "T", Body = "B", PostType = "TextPost" };
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateNewsBlogPostModel>(), default))
            .ReturnsAsync(new ValidationResult());

        var entity = new NewsBlogPostEntity { Id = Guid.NewGuid(), BrewerId = model.BrewerId, Title = model.Title, Body = model.Body, PostType = Beers.Domain.Enums.NewsBlogPostType.TextPost };
        _mockHydration.Setup(x => x.HydrateEntity(It.IsAny<CreateNewsBlogPostModel>())).ReturnsAsync(entity);

        // mock DbSet backed by list
        var list = new List<NewsBlogPostEntity> { entity };
        var mockDbSet = list.BuildMockDbSet<NewsBlogPostEntity>();

        // create a fake DbContext that overrides SaveChangesAsync to avoid provider requirement
        var options = new DbContextOptionsBuilder<BeersDbContext>().Options;
        await using var context = new FakeBeersDbContext(options);
        context.NewsBlogPostEntities = mockDbSet.Object;

        _mockDbFactory.Setup(x => x.CreateDbContextAsync()).ReturnsAsync(context);

        var sut = new CreateNewsBlogPostService(_mapper, _mockValidator.Object, _mockHydration.Object, _mockDbFactory.Object);

        // act
        var (resultModel, errors) = await sut.CreateAsync(model);

        // assert
        errors.Count.Should().Be(0);
        resultModel.NewsBlogPostId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsync_returns_errors_when_validation_fails()
    {
        var model = new CreateNewsBlogPostModel { BrewerId = Guid.NewGuid(), Title = "T", Body = "B", PostType = "TextPost" };
        var failures = new List<ValidationFailure> { new ValidationFailure("Model", "Invalid") };
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<CreateNewsBlogPostModel>(), default)).ReturnsAsync(new ValidationResult(failures));

        var sut = new CreateNewsBlogPostService(_mapper, _mockValidator.Object, _mockHydration.Object, _mockDbFactory.Object);

        var (resultModel, errors) = await sut.CreateAsync(model);

        errors.Count.Should().BeGreaterThan(0);
        resultModel.Should().NotBeNull();
    }

    private sealed class FakeBeersDbContext : BeersDbContext
    {
        public FakeBeersDbContext(DbContextOptions<BeersDbContext> options) : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);
    }
}

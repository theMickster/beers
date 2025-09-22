using AutoMapper;
using Beers.Application.Data;
using Beers.Application.Interfaces.Services.Hydration;
using Beers.Application.Services.NewsBlogPost;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Beers.Domain.Models.NewsBlogPost;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.Moq;

namespace Beers.UnitTests.Services.NewsBlogPost;

public sealed class UpdateNewsBlogPostServiceTests
{
    private readonly Mock<IValidator<UpdateNewsBlogPostModel>> _mockValidator = new();
    private readonly Mock<INewsBlogPostHydrationService> _mockHydration = new();
    private readonly Mock<IDbContextFactory<BeersDbContext>> _mockDbFactory = new();
    private readonly IMapper _mapper;

    public UpdateNewsBlogPostServiceTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<Beers.Domain.Profiles.NewsBlogPostEntityToModelProfile>();
        }, NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task UpdateAsync_returns_model_when_valid()
    {
        // arrange
        var brewerId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var model = new UpdateNewsBlogPostModel
        {
            BrewerId = brewerId,
            NewsBlogPostId = postId,
            Title = "Updated Title",
            Body = "Updated Body",
            PostType = "TextPost"
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<UpdateNewsBlogPostModel>(), default))
            .ReturnsAsync(new ValidationResult());

        var existingEntity = new NewsBlogPostEntity
        {
            Id = postId,
            BrewerId = brewerId,
            Title = "Original Title",
            Body = "Original Body",
            PostType = Beers.Domain.Enums.NewsBlogPostType.TextPost,
            CreatedDate = DateTime.UtcNow.AddDays(-1),
            ModifiedDate = DateTime.UtcNow.AddDays(-1)
        };

        var updatedEntity = new NewsBlogPostEntity
        {
            Id = postId,
            BrewerId = brewerId,
            Title = model.Title,
            Body = model.Body,
            PostType = Beers.Domain.Enums.NewsBlogPostType.TextPost,
            Author = new BrewerSlimEntity { Id = brewerId, Name = "Test Brewer", Website = "https://test.com" }
        };

        _mockHydration.Setup(x => x.HydrateEntity(It.IsAny<UpdateNewsBlogPostModel>())).ReturnsAsync(updatedEntity);

        // mock DbSet backed by list
        var list = new List<NewsBlogPostEntity> { existingEntity };
        var mockDbSet = list.BuildMockDbSet<NewsBlogPostEntity>();

        // create a fake DbContext that overrides SaveChangesAsync to avoid provider requirement
        var options = new DbContextOptionsBuilder<BeersDbContext>().Options;
        await using var context = new FakeBeersDbContext(options);
        context.NewsBlogPostEntities = mockDbSet.Object;

        _mockDbFactory.Setup(x => x.CreateDbContextAsync()).ReturnsAsync(context);

        var sut = new UpdateNewsBlogPostService(_mapper, _mockValidator.Object, _mockHydration.Object, _mockDbFactory.Object);

        // act
        var (resultModel, errors) = await sut.UpdateAsync(model);

        // assert
        using (new AssertionScope())
        {
            errors.Count.Should().Be(0);
            resultModel.NewsBlogPostId.Should().Be(postId);
            resultModel.BrewerId.Should().Be(brewerId);
            resultModel.Title.Should().Be("Updated Title");
        }
    }

    [Fact]
    public async Task UpdateAsync_returns_errors_when_validation_fails()
    {
        var model = new UpdateNewsBlogPostModel
        {
            BrewerId = Guid.NewGuid(),
            NewsBlogPostId = Guid.NewGuid(),
            Title = "T",
            Body = "B",
            PostType = "TextPost"
        };
        var failures = new List<ValidationFailure> { new ValidationFailure("Model", "Invalid") };
        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<UpdateNewsBlogPostModel>(), default)).ReturnsAsync(new ValidationResult(failures));

        var sut = new UpdateNewsBlogPostService(_mapper, _mockValidator.Object, _mockHydration.Object, _mockDbFactory.Object);

        var (resultModel, errors) = await sut.UpdateAsync(model);

        using (new AssertionScope())
        {
            errors.Count.Should().BeGreaterThan(0);
            resultModel.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task UpdateAsync_returns_error_when_post_not_found()
    {
        // arrange
        var brewerId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var model = new UpdateNewsBlogPostModel
        {
            BrewerId = brewerId,
            NewsBlogPostId = postId,
            Title = "Updated Title",
            Body = "Updated Body",
            PostType = "TextPost"
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<UpdateNewsBlogPostModel>(), default))
            .ReturnsAsync(new ValidationResult());

        // mock DbSet with empty list (post not found)
        var list = new List<NewsBlogPostEntity>();
        var mockDbSet = list.BuildMockDbSet<NewsBlogPostEntity>();

        var options = new DbContextOptionsBuilder<BeersDbContext>().Options;
        await using var context = new FakeBeersDbContext(options);
        context.NewsBlogPostEntities = mockDbSet.Object;

        _mockDbFactory.Setup(x => x.CreateDbContextAsync()).ReturnsAsync(context);

        var sut = new UpdateNewsBlogPostService(_mapper, _mockValidator.Object, _mockHydration.Object, _mockDbFactory.Object);

        // act
        var (resultModel, errors) = await sut.UpdateAsync(model);

        // assert
        using (new AssertionScope())
        {
            errors.Count.Should().BeGreaterThan(0);
            errors.Should().Contain(e => e.PropertyName == "NewsBlogPostId");
        }
    }

    [Fact]
    public async Task UpdateAsync_refreshes_modified_date()
    {
        // arrange
        var brewerId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var originalModifiedDate = DateTime.UtcNow.AddDays(-5);

        var model = new UpdateNewsBlogPostModel
        {
            BrewerId = brewerId,
            NewsBlogPostId = postId,
            Title = "Updated Title",
            Body = "Updated Body",
            PostType = "TextPost"
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<UpdateNewsBlogPostModel>(), default))
            .ReturnsAsync(new ValidationResult());

        var existingEntity = new NewsBlogPostEntity
        {
            Id = postId,
            BrewerId = brewerId,
            Title = "Original Title",
            Body = "Original Body",
            PostType = Beers.Domain.Enums.NewsBlogPostType.TextPost,
            CreatedDate = DateTime.UtcNow.AddDays(-10),
            ModifiedDate = originalModifiedDate
        };

        var updatedEntity = new NewsBlogPostEntity
        {
            Id = postId,
            BrewerId = brewerId,
            Title = model.Title,
            Body = model.Body,
            PostType = Beers.Domain.Enums.NewsBlogPostType.TextPost,
            Author = new BrewerSlimEntity { Id = brewerId, Name = "Test Brewer", Website = "https://test.com" }
        };

        _mockHydration.Setup(x => x.HydrateEntity(It.IsAny<UpdateNewsBlogPostModel>())).ReturnsAsync(updatedEntity);

        var list = new List<NewsBlogPostEntity> { existingEntity };
        var mockDbSet = list.BuildMockDbSet<NewsBlogPostEntity>();

        var options = new DbContextOptionsBuilder<BeersDbContext>().Options;
        await using var context = new FakeBeersDbContext(options);
        context.NewsBlogPostEntities = mockDbSet.Object;

        _mockDbFactory.Setup(x => x.CreateDbContextAsync()).ReturnsAsync(context);

        var sut = new UpdateNewsBlogPostService(_mapper, _mockValidator.Object, _mockHydration.Object, _mockDbFactory.Object);

        // act
        var (resultModel, errors) = await sut.UpdateAsync(model);

        // assert
        using (new AssertionScope())
        {
            errors.Count.Should().Be(0);
            existingEntity.ModifiedDate.Should().BeAfter(originalModifiedDate);
        }
    }

    private sealed class FakeBeersDbContext : BeersDbContext
    {
        public FakeBeersDbContext(DbContextOptions<BeersDbContext> options) : base(options)
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(1);

        public override Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        {
            // Return a mock entry to avoid "No database provider configured" error in unit tests
            return null!;
        }
    }
}

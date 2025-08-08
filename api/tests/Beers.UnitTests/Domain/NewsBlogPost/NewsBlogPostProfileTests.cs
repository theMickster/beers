using AutoMapper;
using Beers.Common.Constants;
using Beers.Domain.Entities;
using Beers.Domain.Entities.Base;
using Beers.Domain.Enums;
using Beers.Domain.Models.NewsBlogPost;
using Beers.Domain.Profiles;
using Microsoft.Extensions.Logging.Abstractions;

namespace Beers.UnitTests.Domain.NewsBlogPost;

public sealed class NewsBlogPostProfileTests
{
    private readonly IMapper mapper;

    public NewsBlogPostProfileTests()
    {
        var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<NewsBlogPostCreateModelToEntityProfile>();
                cfg.AddProfile<NewsBlogPostEntityToModelProfile>();
            },
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
        mapper = configuration.CreateMapper();
    }

    [Fact]
    public void NewsBlogPostEntity_ShouldExposeExpectedEntityType()
    {
        var entity = new NewsBlogPostEntity();

        entity.EntityType.Should().Be(PartitionKeyConstants.NewsBlogPost);
    }

    [Fact]
    public void CreateMap_ShouldMapCreateModelToEntity()
    {
        var brewerId = Guid.NewGuid();
        var publishedDate = DateTime.UtcNow;
        var eventDate = publishedDate.AddDays(7);
        var model = new CreateNewsBlogPostModel
        {
            BrewerId = brewerId,
            Title = "Beer festival weekend",
            Body = "Join us for a tap takeover.",
            PostType = NewsBlogPostType.EventAnnouncement.ToString(),
            Tags = ["festival", "weekend"],
            ImageUrls = ["https://cdn.example.com/1.jpg", "https://cdn.example.com/2.jpg"],
            EventDate = eventDate,
            EventLocation = "Tap Room",
            PublishedDate = publishedDate
        };

        var entity = mapper.Map<NewsBlogPostEntity>(model);

        using (new AssertionScope())
        {
            entity.BrewerId.Should().Be(brewerId);
            entity.Title.Should().Be(model.Title);
            entity.Body.Should().Be(model.Body);
            entity.PostType.Should().Be(NewsBlogPostType.EventAnnouncement);
            entity.Tags.Should().BeEquivalentTo(model.Tags, options => options.WithStrictOrdering());
            entity.ImageUrls.Should().BeEquivalentTo(model.ImageUrls, options => options.WithStrictOrdering());
            entity.EventDate.Should().Be(eventDate);
            entity.EventLocation.Should().Be(model.EventLocation);
            entity.PublishedDate.Should().Be(publishedDate);
            entity.Author.Name.Should().BeEmpty("Author must not be mapped from client input");
            entity.Author.Website.Should().BeEmpty("Author must not be mapped from client input");
            entity.Author.Id.Should().Be(Guid.Empty, "Author.Id must default to unhydrated state");
            entity.IsDeletable.Should().BeTrue("IsDeletable must not be mapped from client input");
        }
    }

    [Fact]
    public void ReadMap_ShouldMapEntityToReadModel()
    {
        var newsBlogPostId = Guid.NewGuid();
        var brewerId = Guid.NewGuid();
        var publishedDate = DateTime.UtcNow;
        var eventDate = publishedDate.AddDays(5);
        var entity = new NewsBlogPostEntity
        {
            Id = newsBlogPostId,
            BrewerId = brewerId,
            Title = "Summer release",
            Body = "Fresh cans are available now.",
            PostType = NewsBlogPostType.ImageGallery,
            Tags = ["summer", "release"],
            ImageUrls = ["https://cdn.example.com/release.jpg"],
            EventDate = eventDate,
            EventLocation = "Downtown",
            PublishedDate = publishedDate,
            Author = new BrewerSlimEntity
            {
                Id = Guid.NewGuid(),
                Name = "North Harbor Brewing",
                Website = "https://northharbor.example.com"
            }
        };

        var model = mapper.Map<ReadNewsBlogPostModel>(entity);

        using (new AssertionScope())
        {
            model.NewsBlogPostId.Should().Be(newsBlogPostId);
            model.BrewerId.Should().Be(brewerId);
            model.Title.Should().Be(entity.Title);
            model.Body.Should().Be(entity.Body);
            model.PostType.Should().Be(NewsBlogPostType.ImageGallery.ToString());
            model.Tags.Should().BeEquivalentTo(entity.Tags, options => options.WithStrictOrdering());
            model.ImageUrls.Should().BeEquivalentTo(entity.ImageUrls, options => options.WithStrictOrdering());
            model.EventDate.Should().Be(eventDate);
            model.EventLocation.Should().Be(entity.EventLocation);
            model.PublishedDate.Should().Be(publishedDate);
            model.Author.Id.Should().Be(entity.Author.Id);
            model.Author.Name.Should().Be(entity.Author.Name);
            model.Author.Website.Should().Be(entity.Author.Website);
            model.IsDeletable.Should().Be(entity.IsDeletable);
        }
    }
}

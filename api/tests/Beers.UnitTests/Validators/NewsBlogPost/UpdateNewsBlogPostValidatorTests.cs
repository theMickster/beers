using Beers.Application.Interfaces.Services.Brewer;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Application.Validators.NewsBlogPost;
using Beers.Domain.Models.Brewer;
using Beers.Domain.Models.NewsBlogPost;
using Beers.UnitTests.Common;

namespace Beers.UnitTests.Validators.NewsBlogPost;

public sealed class UpdateNewsBlogPostValidatorTests
{
    private readonly UpdateNewsBlogPostValidator _sut;
    private readonly Mock<IReadBrewerService> _readBrewerService = new();
    private readonly Mock<IReadNewsBlogPostService> _readNewsBlogPostService = new();

    public UpdateNewsBlogPostValidatorTests()
    {
        _readBrewerService.Setup(x => x.GetByIdAsync(It.Is<Guid>(g => g == BeerFixtures.BrewerNewBelgium)))
            .ReturnsAsync(new ReadBrewerModel { BrewerId = BeerFixtures.BrewerNewBelgium, Name = "New Belgium" });
        _readBrewerService.Setup(x => x.GetByIdAsync(It.Is<Guid>(g => g != BeerFixtures.BrewerNewBelgium)))
            .ReturnsAsync((ReadBrewerModel?)null);

        _readNewsBlogPostService.Setup(x => x.GetByIdAsync(
                It.Is<Guid>(g => g == BeerFixtures.BrewerNewBelgium),
                It.Is<Guid>(p => p == BeerFixtures.NewsBlogPostId1)))
            .ReturnsAsync(new ReadNewsBlogPostModel { NewsBlogPostId = BeerFixtures.NewsBlogPostId1, BrewerId = BeerFixtures.BrewerNewBelgium });

        _readNewsBlogPostService.Setup(x => x.GetByIdAsync(
                It.Is<Guid>(g => g != BeerFixtures.BrewerNewBelgium || g == Guid.Empty),
                It.IsAny<Guid>()))
            .ReturnsAsync((ReadNewsBlogPostModel?)null);

        _readNewsBlogPostService.Setup(x => x.GetByIdAsync(
                It.Is<Guid>(g => g == BeerFixtures.BrewerNewBelgium),
                It.Is<Guid>(p => p != BeerFixtures.NewsBlogPostId1)))
            .ReturnsAsync((ReadNewsBlogPostModel?)null);

        _sut = new UpdateNewsBlogPostValidator(_readBrewerService.Object, _readNewsBlogPostService.Object);
    }

    [Fact]
    public async Task Validator_succeeds()
    {
        var model = new UpdateNewsBlogPostModel
        {
            BrewerId = BeerFixtures.BrewerNewBelgium,
            NewsBlogPostId = BeerFixtures.NewsBlogPostId1,
            Title = "Updated Title",
            Body = "Updated body content",
            PostType = "TextPost"
        };

        var result = await _sut.ValidateAsync(model);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_fails_when_newsblogpostid_is_empty()
    {
        var model = CreateValidModel();
        model.NewsBlogPostId = Guid.Empty;

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "NewsBlogPostId").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_fails_when_brewer_id_is_empty()
    {
        var model = CreateValidModel();
        model.BrewerId = Guid.Empty;

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "BrewerId").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_fails_when_post_does_not_exist_for_brewer()
    {
        var model = new UpdateNewsBlogPostModel
        {
            BrewerId = BeerFixtures.BrewerNewBelgium,
            NewsBlogPostId = Guid.NewGuid(), // Different post ID
            Title = "Updated Title",
            Body = "Updated body",
            PostType = "TextPost"
        };

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Any(x => x.PropertyName == "NewsBlogPostId").Should().BeTrue();
        }
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("Blog")]
    [InlineData("text_post")]
    public async Task Validator_fails_for_invalid_post_type(string postType)
    {
        var model = CreateValidModel();
        model.PostType = postType;

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "PostType").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    public async Task Validator_fails_when_post_type_is_empty(string postType)
    {
        var model = CreateValidModel();
        model.PostType = postType;

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "PostType").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_fails_for_eventannouncement_missing_event_date()
    {
        var model = CreateValidModel();
        model.PostType = "EventAnnouncement";
        model.EventDate = null;
        model.EventLocation = "Some Location";

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Any(x => x.PropertyName == "EventDate").Should().BeTrue();
        }
    }

    [Fact]
    public async Task Validator_fails_for_eventannouncement_missing_event_location()
    {
        var model = CreateValidModel();
        model.PostType = "EventAnnouncement";
        model.EventDate = DateTime.UtcNow.AddDays(1);
        model.EventLocation = null;

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Any(x => x.PropertyName == "EventLocation").Should().BeTrue();
        }
    }

    [Fact]
    public async Task Validator_fails_for_imagegallery_with_no_images()
    {
        var model = CreateValidModel();
        model.PostType = "ImageGallery";
        model.ImageUrls = new List<string>();

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Any(x => x.PropertyName == "ImageUrls").Should().BeTrue();
        }
    }

    [Fact]
    public async Task Validator_succeeds_for_textpost()
    {
        var model = CreateValidModel();
        model.PostType = "TextPost";
        model.Body = "This is the text body";

        var result = await _sut.ValidateAsync(model);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_succeeds_for_imagegallery()
    {
        var model = CreateValidModel();
        model.PostType = "ImageGallery";
        model.Body = null;
        model.ImageUrls = new List<string> { "https://cdn.example.com/1.jpg" };

        var result = await _sut.ValidateAsync(model);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_succeeds_for_eventannouncement()
    {
        var model = CreateValidModel();
        model.PostType = "EventAnnouncement";
        model.Body = null;
        model.EventDate = DateTime.UtcNow.AddDays(1);
        model.EventLocation = "Brewery Tap Room";

        var result = await _sut.ValidateAsync(model);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    public async Task Validator_fails_when_title_is_empty(string title)
    {
        var model = CreateValidModel();
        model.Title = title;

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "Title").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_fails_when_title_is_too_long()
    {
        var model = CreateValidModel();
        model.Title = StringGenerator.GetRandomString(257);

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "Title").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    private static UpdateNewsBlogPostModel CreateValidModel() => new()
    {
        BrewerId = BeerFixtures.BrewerNewBelgium,
        NewsBlogPostId = BeerFixtures.NewsBlogPostId1,
        Title = "Updated Title",
        Body = "Updated body content",
        PostType = "TextPost"
    };
}

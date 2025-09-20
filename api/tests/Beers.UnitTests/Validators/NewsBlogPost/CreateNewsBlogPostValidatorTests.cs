using Beers.Application.Interfaces.Services.Brewer;
using Beers.Application.Validators.NewsBlogPost;
using Beers.Domain.Models.Brewer;
using Beers.Domain.Models.NewsBlogPost;
using Beers.UnitTests.Common;

namespace Beers.UnitTests.Validators.NewsBlogPost;

public sealed class CreateNewsBlogPostValidatorTests
{
    private readonly CreateNewsBlogPostValidator _sut;
    private readonly Mock<IReadBrewerService> _readBrewerService = new();

    public CreateNewsBlogPostValidatorTests()
    {
        _readBrewerService.Setup(x => x.GetByIdAsync(It.Is<Guid>(g => g == BeerFixtures.BrewerNewBelgium)))
            .ReturnsAsync(new ReadBrewerModel { BrewerId = BeerFixtures.BrewerNewBelgium, Name = "New Belgium" });
        _readBrewerService.Setup(x => x.GetByIdAsync(It.Is<Guid>(g => g != BeerFixtures.BrewerNewBelgium)))
            .ReturnsAsync((ReadBrewerModel?)null);

        _sut = new CreateNewsBlogPostValidator(_readBrewerService.Object);
    }

    [Fact]
    public async Task Validator_succeeds()
    {
        var model = new CreateNewsBlogPostModel
        {
            BrewerId = BeerFixtures.BrewerNewBelgium,
            Title = "Exciting Brew Release",
            Body = "We are thrilled to announce our newest seasonal IPA.",
            PostType = "TextPost"
        };

        var result = await _sut.ValidateAsync(model);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("TextPost")]
    [InlineData("ImageGallery")]
    [InlineData("EventAnnouncement")]
    [InlineData("textpost")]
    [InlineData("eventannouncement")]
    public async Task Validator_succeeds_for_valid_post_types(string postType)
    {
        var model = CreateValidModel();
        model.PostType = postType;

        if (string.Equals(postType, "EventAnnouncement", StringComparison.OrdinalIgnoreCase))
        {
            model.EventDate = DateTime.UtcNow.AddDays(1);
            model.EventLocation = "Test Location";
        }

        if (string.Equals(postType, "ImageGallery", StringComparison.OrdinalIgnoreCase))
        {
            model.ImageUrls = new List<string> { "https://cdn.example.com/1.jpg" };
        }

        var result = await _sut.ValidateAsync(model);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("Blog")]
    [InlineData("text_post")]
    [InlineData("123")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("2")]
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

    [Fact]
    public async Task Validator_fails_when_post_type_is_omitted()
    {
        var model = new CreateNewsBlogPostModel
        {
            BrewerId = BeerFixtures.BrewerNewBelgium,
            Title = "Exciting Brew Release",
            Body = "We are thrilled to announce our newest seasonal IPA."
        };

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

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
#pragma warning disable xUnit1012
    [InlineData(null)]
#pragma warning restore xUnit1012
    public async Task Validator_fails_when_body_is_empty(string body)
    {
        var model = CreateValidModel();
        model.PostType = "TextPost";
        model.Body = body;

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "Body").Should().BeGreaterThanOrEqualTo(1);
        }
    }

    [Fact]
    public async Task Validator_allows_empty_body_for_imagegallery_and_eventannouncement()
    {
        // ImageGallery with images should pass even when Body is empty
        var imageModel = CreateValidModel();
        imageModel.PostType = "ImageGallery";
        imageModel.Body = null;
        imageModel.ImageUrls = new List<string> { "https://cdn.example.com/1.jpg" };

        var imageResult = await _sut.ValidateAsync(imageModel);
        imageResult.IsValid.Should().BeTrue();

        // EventAnnouncement with required fields should pass even when Body is empty
        var eventModel = CreateValidModel();
        eventModel.PostType = "EventAnnouncement";
        eventModel.Body = null;
        eventModel.EventDate = DateTime.UtcNow.AddDays(1);
        eventModel.EventLocation = "Test Location";

        var eventResult = await _sut.ValidateAsync(eventModel);
        eventResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_fails_when_body_is_too_long()
    {
        var model = CreateValidModel();
        model.Body = StringGenerator.GetRandomString(4001);

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Count(x => x.PropertyName == "Body").Should().BeGreaterThanOrEqualTo(1);
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
    public async Task Validator_fails_when_brewer_does_not_exist()
    {
        var model = new CreateNewsBlogPostModel
        {
            BrewerId = Guid.NewGuid(),
            Title = "Exciting Brew Release",
            Body = "We are thrilled to announce our newest seasonal IPA.",
            PostType = "TextPost"
        };

        var result = await _sut.ValidateAsync(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Any(x => x.PropertyName == "BrewerId").Should().BeTrue();
    }

    private static CreateNewsBlogPostModel CreateValidModel() => new()
    {
        BrewerId = BeerFixtures.BrewerNewBelgium,
        Title = "Exciting Brew Release",
        Body = "We are thrilled to announce our newest seasonal IPA.",
        PostType = "TextPost"
    };

    [Fact]
    public async Task Validator_fails_for_eventannouncement_missing_required_fields()
    {
        var model = CreateValidModel();
        model.PostType = "EventAnnouncement";
        model.EventDate = null;
        model.EventLocation = null;

        var result = await _sut.ValidateAsync(model);
        using (new AssertionScope())
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Any(x => x.PropertyName == "EventDate").Should().BeTrue();
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
}

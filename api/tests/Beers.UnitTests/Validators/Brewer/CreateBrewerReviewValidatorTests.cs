using Beers.Application.Interfaces.Services.Brewer;
using Beers.Application.Validators.Brewer;
using Beers.Domain.Models.Brewer;
using Beers.UnitTests.Common;

namespace Beers.UnitTests.Validators.Brewer;

public sealed class CreateBrewerReviewValidatorTests
{
    private readonly CreateBrewerReviewValidator _sut;
    private readonly Mock<IReadBrewerService> _readBrewerService = new();

    public CreateBrewerReviewValidatorTests()
    {
        _readBrewerService.Setup(x => x.GetByIdAsync(It.Is<Guid>(g => g == BeerFixtures.BrewerNewBelgium)))
            .ReturnsAsync(new ReadBrewerModel { BrewerId = BeerFixtures.BrewerNewBelgium, Name = "New Belgium" });
        _readBrewerService.Setup(x => x.GetByIdAsync(It.Is<Guid>(g => g != BeerFixtures.BrewerNewBelgium)))
            .ReturnsAsync((ReadBrewerModel?)null);

        _sut = new CreateBrewerReviewValidator(_readBrewerService.Object);
    }

    [Fact]
    public async Task Validator_succeeds()
    {
        var model = new CreateBrewerReviewModel
        {
            BrewerId = BeerFixtures.BrewerNewBelgium,
            ReviewerName = "Mick",
            Title = "Great selection",
            Comments = "Loved the variety and seasonal releases.",
            Rating = 5
        };

        var result = await _sut.ValidateAsync(model);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_fails_when_brewer_does_not_exist()
    {
        var model = new CreateBrewerReviewModel
        {
            BrewerId = Guid.NewGuid(),
            ReviewerName = "Mick",
            Title = "Great selection",
            Comments = "Loved the variety and seasonal releases.",
            Rating = 5
        };

        var result = await _sut.ValidateAsync(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Any(x => x.PropertyName == "BrewerId").Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task Validator_fails_when_rating_out_of_range(int rating)
    {
        var model = new CreateBrewerReviewModel
        {
            BrewerId = BeerFixtures.BrewerNewBelgium,
            ReviewerName = "Mick",
            Title = "Great selection",
            Comments = "Loved the variety and seasonal releases.",
            Rating = rating
        };

        var result = await _sut.ValidateAsync(model);
        result.IsValid.Should().BeFalse();
        result.Errors.Any(x => x.PropertyName == "Rating").Should().BeTrue();
    }
}

using AutoMapper;
using Beers.API.Controllers.v1.Beer;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Domain.Models.Beer;
using Beers.Domain.Profiles;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Beers.Domain.Models.Metadata;
using Beers.UnitTests.Common;

namespace Beers.UnitTests.Controllers.v1.Beer;

public sealed class UpdateBeerControllerTests
{
    private readonly UpdateBeerController _sut;
    private readonly Mock<ILogger<UpdateBeerController>> _logger = new();
    private readonly Mock<IUpdateBeerService> _mockUpdateBeerService = new();

    public UpdateBeerControllerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(BeerCategoryEntityToModelProfile).Assembly)
        );

        var mapper = mappingConfig.CreateMapper();

        _sut = new UpdateBeerController(_logger.Object, _mockUpdateBeerService.Object);
    }

    [Fact]
    public async Task PutAsync_bad_request_when_model_is_null()
    {
        var result = await _sut.PutAsync(Guid.NewGuid(), null);
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("Unable to update beer because of an invalid input model.");
        }
    }

    [Fact]
    public async Task PutAsync_bad_request_when_model_is_invalid()
    {
        _sut.ModelState.AddModelError("BeerId", "Something went wrong");
        var result = await _sut.PutAsync(Guid.NewGuid(), new UpdateBeerModel());
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("Unable to update beer because of an invalid input model.");
        }
    }
    
    [Fact]
    public async Task PutAsync_bad_request_when_ids_do_not_match()
    {
        var beerId = Guid.NewGuid();
        var updateId = Guid.NewGuid();

        var result = await _sut.PutAsync(updateId, new UpdateBeerModel { BeerId = beerId });
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("The beer id parameter must match the id of the beer update request payload.");
        }
    }

    [Fact]
    public async Task PutAsync_bad_request_when_model_has_validation_errors()
    {
        var beerId = Guid.NewGuid();
        _mockUpdateBeerService.Setup(x => x.UpdateAsync(It.IsAny<UpdateBeerModel>()))
            .ReturnsAsync((new ReadBeerModel(), [new ValidationFailure { ErrorCode = "01", ErrorMessage = "SomeError" }]));

        var result = await _sut.PutAsync(beerId, new UpdateBeerModel{BeerId = beerId });
        var objectResult = result.Result as BadRequestObjectResult;
        var output = objectResult?.Value as IEnumerable<string>;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.ToList().Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task PutAsync_succeeds()
    {
        var beerId = Guid.NewGuid();
        var brewerId = Guid.NewGuid();
        _mockUpdateBeerService.Setup(x => x.UpdateAsync(It.IsAny<UpdateBeerModel>()))
            .ReturnsAsync((new ReadBeerModel
            {
                BeerId = beerId,
                BrewerId = brewerId,
                Name = "A",
                Description = "B",
                Image = "C",
                Sku = "D",
                Rating = new RatingModel{ReviewCount = 2, Average = 2.5m},
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                BeerType = MetadataFixtures.GetBeerTypeModels()[0],
                Brewer = new BrewerModel(),
                Pricing = [new PriceModel()],
                BeerCategories = [new BeerCategoryModel()],
                BeerStyles = [new BeerStyleModel()]
            }, []));

        var result = await _sut.PutAsync(beerId, new UpdateBeerModel { BeerId = beerId });
        var objectResult = result.Result as OkObjectResult;
        var output = objectResult.Value as ReadBeerModel;

        using (new AssertionScope())
        {
            objectResult.Should().BeOfType<OkObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
            output!.Should().NotBeNull();
            output!.BrewerId.Should().NotBe(Guid.Empty);
            output!.Name.Should().Be("A");
            output!.Description.Should().Be("B");
            output!.Image.Should().Be("C");
            output!.Sku.Should().Be("D");
            output!.CreatedDate.Should().BeOnOrAfter(DateTime.UtcNow.AddSeconds(-2));
            output!.ModifiedDate.Should().BeOnOrAfter(DateTime.UtcNow.AddSeconds(-2));
            output!.BeerType.Should().NotBeNull();
            output!.Rating.Should().NotBeNull();
            output!.Brewer.Should().NotBeNull();
            output!.Pricing.Should().NotBeNullOrEmpty();
            output!.BeerCategories.Should().NotBeNullOrEmpty();
            output!.BeerStyles.Should().NotBeNullOrEmpty();
        }
    }
}

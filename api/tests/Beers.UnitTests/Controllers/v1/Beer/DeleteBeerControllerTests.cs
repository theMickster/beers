using Beers.API.Controllers.v1.Beer;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Constants;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.Beer;
public sealed class DeleteBeerControllerTests
{
    private readonly DeleteBeerController _sut;
    private readonly Mock<ILogger<DeleteBeerController>> _logger = new();
    private readonly Mock<IDeleteBeerService> _deleteBeerService = new();

    public DeleteBeerControllerTests()
    {
        _sut = new DeleteBeerController(_logger.Object, _deleteBeerService.Object);
    }

    [Fact]
    public async Task DeleteAsync_bad_request_when_model_is_invalid()
    {
        _sut.ModelState.AddModelError("Id", "Something went wrong");

        var result = await _sut.DeleteAsync(Guid.NewGuid());
        var objectResult = result as BadRequestObjectResult;
        var output = objectResult?.Value as string;

        using (new AssertionScope())
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.Should().Be("Unable to delete beer because of an invalid input model.");
        }
    }

    [Fact]
    public async Task DeleteAsync_bad_request_when_model_has_validation_errors()
    {
        _deleteBeerService.Setup(x => x.DeleteAsync(It.IsAny<Guid>()))
            .ReturnsAsync((false, [new ValidationFailure() { ErrorCode = "ABC123", ErrorMessage = "Something" }]));

        var result = await _sut.DeleteAsync(Guid.NewGuid());
        var objectResult = result as BadRequestObjectResult;
        var output = objectResult?.Value as IEnumerable<string>;

        using (new AssertionScope())
        {
            result.Should().BeOfType<BadRequestObjectResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            output!.Should().NotBeNullOrEmpty();
            output!.ToList().Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task DeleteAsync_not_found_when_entity_does_not_exist()
    {
        _deleteBeerService.Setup(x => x.DeleteAsync(It.IsAny<Guid>()))
            .ReturnsAsync((false, [new ValidationFailure() { ErrorCode = ValidatorConstants.NotFoundErrorCode, ErrorMessage = "Not Found" }]));
        
        var result = await _sut.DeleteAsync(Guid.NewGuid());
        var objectResult = result as NotFoundResult;

        using (new AssertionScope())
        {
            result.Should().BeOfType<NotFoundResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task DeleteAsync_succeeds()
    {
        _deleteBeerService.Setup(x => x.DeleteAsync(It.IsAny<Guid>()))
            .ReturnsAsync((true, []));

        var result = await _sut.DeleteAsync(Guid.NewGuid());
        var objectResult = result as NoContentResult;

        using (new AssertionScope())
        {
            result.Should().BeOfType<NoContentResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }
    }
}

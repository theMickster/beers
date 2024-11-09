using Beers.API.Controllers.v1.Brewer;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Constants;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.Brewer;

public sealed class DeleteBrewerControllerTests
{
    private readonly DeleteBrewerController _sut;
    private readonly Mock<ILogger<DeleteBrewerController>> _logger = new();
    private readonly Mock<IDeleteBrewerService> _deleteBrewerService = new();

    public DeleteBrewerControllerTests()
    {
        _sut = new DeleteBrewerController(_logger.Object, _deleteBrewerService.Object);
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
            output!.Should().Be("Unable to delete brewer because of an invalid input model.");
        }
    }

    [Fact]
    public async Task DeleteAsync_bad_request_when_model_has_validation_errors()
    {
        _deleteBrewerService.Setup(x => x.DeleteAsync(It.IsAny<Guid>()))
            .ReturnsAsync((false, [new ValidationFailure { ErrorCode = "ABC123", ErrorMessage = "Something Else" }]));

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
        _deleteBrewerService.Setup(x => x.DeleteAsync(It.IsAny<Guid>()))
            .ReturnsAsync((false, [new ValidationFailure { ErrorCode = ValidatorConstants.NotFoundErrorCode, ErrorMessage = "Not Found" }]));

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
        _deleteBrewerService.Setup(x => x.DeleteAsync(It.IsAny<Guid>()))
            .ReturnsAsync( (true, []));

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

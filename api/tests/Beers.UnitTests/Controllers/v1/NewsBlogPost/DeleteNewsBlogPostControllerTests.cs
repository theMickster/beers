using Beers.API.Controllers.v1.NewsBlogPost;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Common.Constants;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Beers.UnitTests.Controllers.v1.NewsBlogPost;

public sealed class DeleteNewsBlogPostControllerTests
{
    private readonly DeleteNewsBlogPostController _sut;
    private readonly Mock<ILogger<DeleteNewsBlogPostController>> _logger = new();
    private readonly Mock<IDeleteNewsBlogPostService> _deleteNewsBlogPostService = new();

    public DeleteNewsBlogPostControllerTests()
    {
        _sut = new DeleteNewsBlogPostController(_logger.Object, _deleteNewsBlogPostService.Object);
    }

    [Fact]
    public async Task DeleteAsync_not_found_when_entity_does_not_exist()
    {
        _deleteNewsBlogPostService.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((false, [new ValidationFailure { ErrorCode = ValidatorConstants.NotFoundErrorCode, ErrorMessage = "Not Found" }]));

        var result = await _sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());
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
        _deleteNewsBlogPostService.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((true, []));

        var result = await _sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());
        var objectResult = result as NoContentResult;

        using (new AssertionScope())
        {
            result.Should().BeOfType<NoContentResult>();
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }
    }
}

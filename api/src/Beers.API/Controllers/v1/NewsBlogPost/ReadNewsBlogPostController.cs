using Asp.Versioning;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Domain.Models.NewsBlogPost;
using Microsoft.AspNetCore.Mvc;

namespace Beers.API.Controllers.v1.NewsBlogPost;

/// <summary>
/// Handles reading news/blog posts for a brewer.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "NewsBlogPost")]
[Route("api/v1/brewers/{brewerId:guid}/posts", Name = "Read NewsBlogPost Controller v1")]
[Produces("application/json")]
public sealed class ReadNewsBlogPostController(
    ILogger<ReadNewsBlogPostController> logger,
    IReadNewsBlogPostService readNewsBlogPostService)
    : ControllerBase
{
    private readonly ILogger<ReadNewsBlogPostController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IReadNewsBlogPostService _readNewsBlogPostService = readNewsBlogPostService ?? throw new ArgumentNullException(nameof(readNewsBlogPostService));

    /// <summary>
    /// Retrieve all news/blog posts for a brewer.
    /// </summary>
    [HttpGet]
    [Produces(typeof(List<ReadNewsBlogPostModel>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ReadNewsBlogPostModel>>> GetListAsync(Guid brewerId)
    {
        if (brewerId == Guid.Empty)
        {
            const string message = "Unable to retrieve news/blog posts because of an invalid brewer identifier.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        var model = await _readNewsBlogPostService.GetListAsync(brewerId).ConfigureAwait(false);
        return Ok(model);
    }

    /// <summary>
    /// Retrieve a single news/blog post for a brewer.
    /// </summary>
    [HttpGet("{postId:guid}", Name = "GetNewsBlogPostByIdAsync")]
    [Produces(typeof(ReadNewsBlogPostModel))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadNewsBlogPostModel>> GetByIdAsync(Guid brewerId, Guid postId)
    {
        if (brewerId == Guid.Empty || postId == Guid.Empty)
        {
            const string message = "Unable to retrieve news/blog post because of an invalid identifier.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        var model = await _readNewsBlogPostService.GetByIdAsync(brewerId, postId).ConfigureAwait(false);

        if (model == null)
        {
            _logger.LogInformation("Unable to locate news/blog post model.");
            return NotFound();
        }

        return Ok(model);
    }
}

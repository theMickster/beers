using Asp.Versioning;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Common.Filtering.NewsBlogPost;
using Beers.Domain.Models.NewsBlogPost;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Beers.API.Controllers.v1.NewsBlogPost;

/// <summary>
/// Handles searching news/blog posts across all brewers.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "NewsBlogPost")]
[Route("api/v1/posts", Name = "Search NewsBlogPost Controller v1")]
[Produces("application/json")]
public sealed class SearchNewsBlogPostController(
    ILogger<SearchNewsBlogPostController> logger,
    IReadNewsBlogPostService readNewsBlogPostService) : ControllerBase
{
    private readonly ILogger<SearchNewsBlogPostController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IReadNewsBlogPostService _readNewsBlogPostService = readNewsBlogPostService ?? throw new ArgumentNullException(nameof(readNewsBlogPostService));

    /// <summary>
    /// Accepts filter criteria and returns a paginated list of news/blog posts.
    /// </summary>
    [HttpPost]
    [Route("search", Name = "SearchNewsBlogPostsAsync")]
    [Produces(typeof(SearchResultNewsBlogPostModel))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchResultNewsBlogPostModel>> SearchAsync(
        [FromQuery] SearchNewsBlogPostParameter parameters,
        [FromBody][Required] SearchInputNewsBlogPostModel searchModel)
    {
        if (!ModelState.IsValid)
        {
            const string message = "Unable to search for news/blog posts because of an invalid input model.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        var result = await _readNewsBlogPostService.SearchAsync(parameters, searchModel);

        return Ok(result);
    }
}

using Asp.Versioning;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Common.Constants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Beers.API.Controllers.v1.NewsBlogPost;

/// <summary>
/// The controller that coordinates deleting news/blog post information.
/// </summary>
/// <remarks>
/// The controller that coordinates deleting news/blog post information.
/// </remarks>
/// <param name="logger">Logger for request diagnostics.</param>
/// <param name="deleteNewsBlogPostService">Application service that deletes the post.</param>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "NewsBlogPost")]
[Route("api/v1/brewers/{brewerId:guid}/posts", Name = "Delete NewsBlogPost Controller v1")]
[Produces("application/json")]
public sealed class DeleteNewsBlogPostController(
    ILogger<DeleteNewsBlogPostController> logger,
    IDeleteNewsBlogPostService deleteNewsBlogPostService)
    : ControllerBase
{
    private readonly ILogger<DeleteNewsBlogPostController> _logger = logger ??
        throw new ArgumentNullException(nameof(logger));
    private readonly IDeleteNewsBlogPostService _deleteNewsBlogPostService = deleteNewsBlogPostService ??
        throw new ArgumentNullException(nameof(deleteNewsBlogPostService));

    /// <summary>
    /// Deletes a single news/blog post record.
    /// </summary>
    /// <param name="brewerId">Unique id of a brewer.</param>
    /// <param name="postId">Unique id of a news/blog post.</param>
    /// <returns>An http result.</returns>
    [HttpDelete("{postId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync([Required] Guid brewerId, [Required] Guid postId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Unable to delete news/blog post because of an invalid input model.");
        }

        var (result, errors) = await _deleteNewsBlogPostService.DeleteAsync(brewerId, postId).ConfigureAwait(false);
        _logger.LogInformation("Result of deleting news/blog post with id {PostId} was {Model}", postId, result);

        if (errors.Count == 0)
        {
            return NoContent();
        }

        if (errors.Exists(x => x.ErrorCode == ValidatorConstants.NotFoundErrorCode))
        {
            return NotFound();
        }

        return BadRequest(errors.Select(x => x.ErrorMessage));
    }
}

using Asp.Versioning;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Domain.Models.NewsBlogPost;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace Beers.API.Controllers.v1.NewsBlogPost;

/// <summary>
/// Handles updates to news/blog posts for a brewer.
/// </summary>
/// <param name="logger">Logger for request diagnostics.</param>
/// <param name="updateNewsBlogPostService">Application service that updates the post.</param>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "NewsBlogPost")]
[Route("api/v1/brewers/{brewerId:guid}/posts/{postId:guid}", Name = "Update NewsBlogPost Controller v1")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class UpdateNewsBlogPostController(
    ILogger<UpdateNewsBlogPostController> logger,
    IUpdateNewsBlogPostService updateNewsBlogPostService)
    : ControllerBase
{
    private readonly ILogger<UpdateNewsBlogPostController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IUpdateNewsBlogPostService _updateNewsBlogPostService = updateNewsBlogPostService ?? throw new ArgumentNullException(nameof(updateNewsBlogPostService));

    /// <summary>
    /// Updates an existing news/blog post for the specified brewer.
    /// </summary>
    /// <param name="brewerId">the brewer's id</param>
    /// <param name="postId">the post's id</param>
    /// <param name="inputModel">the news/blog post update details</param>
    /// <returns>200 OK with the updated model, or 400 Bad Request on validation errors</returns>
    [HttpPut]
    [Produces(typeof(ReadNewsBlogPostModel))]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PutAsync(
        [Required] Guid brewerId,
        [Required] Guid postId,
        [FromBody][Required] UpdateNewsBlogPostModel? inputModel)
    {
        if (inputModel == null || !ModelState.IsValid)
        {
            const string message = "Unable to update news/blog post because of an invalid input model.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        if (brewerId != inputModel.BrewerId)
        {
            const string message = "The brewerId parameter must match the BrewerId of the update request payload.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        if (postId != inputModel.NewsBlogPostId)
        {
            const string message = "The postId parameter must match the NewsBlogPostId of the update request payload.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        inputModel.BrewerId = brewerId;
        inputModel.NewsBlogPostId = postId;

        var (model, errors) = await _updateNewsBlogPostService.UpdateAsync(inputModel).ConfigureAwait(false);
        if (errors.Count != 0)
        {
            return BadRequest(errors.Select(x => x.ErrorMessage));
        }

        return Ok(model);
    }
}

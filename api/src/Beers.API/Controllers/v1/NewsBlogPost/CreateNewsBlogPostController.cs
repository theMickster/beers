using Asp.Versioning;
using Beers.Application.Interfaces.Services.NewsBlogPost;
using Beers.Domain.Models.NewsBlogPost;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Beers.API.Controllers.v1.NewsBlogPost;

[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "NewsBlogPost")]
[Route("api/v1/brewers/{brewerId:guid}/posts", Name = "Create NewsBlogPost Controller v1")]
[Produces("application/json")]
public sealed class CreateNewsBlogPostController(
    ILogger<CreateNewsBlogPostController> logger,
    ICreateNewsBlogPostService createNewsBlogPostService)
    : ControllerBase
{
    private readonly ILogger<CreateNewsBlogPostController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICreateNewsBlogPostService _createNewsBlogPostService = createNewsBlogPostService ?? throw new ArgumentNullException(nameof(createNewsBlogPostService));

    /// <summary>
    /// Creates a new news/blog post for the specified brewer.
    /// </summary>
    /// <param name="brewerId">the brewer's id</param>
    /// <param name="inputModel">the news/blog post to create</param>
    /// <returns>201 Created with the created model, or 400 Bad Request on validation errors</returns>
    [HttpPost]
    [Produces(typeof(ReadNewsBlogPostModel))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostAsync(Guid brewerId, [FromBody][Required] CreateNewsBlogPostModel? inputModel)
    {
        if (inputModel == null || !ModelState.IsValid)
        {
            const string message = "Unable to create news/blog post because of an invalid input model.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        inputModel.BrewerId = brewerId;

        var (model, errors) = await _createNewsBlogPostService.CreateAsync(inputModel).ConfigureAwait(false);
        if (errors.Count != 0)
        {
            return BadRequest(errors.Select(x => x.ErrorMessage));
        }

        return StatusCode(StatusCodes.Status201Created, model);
    }
}

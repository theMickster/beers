using Asp.Versioning;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Domain.Models.Brewer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Beers.API.Controllers.v1.BrewerReview;

/// <summary>
/// The controller that coordinates creating Brewer Review information.
/// </summary>
/// <remarks>
/// The controller that coordinates creating Brewer Review information.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "BrewerReview")]
[Route("api/v1/brewers/{brewerId:guid}/reviews", Name = "Create Brewer Review Controller v1")]
[Produces("application/json")]
public sealed class CreateBrewerReviewController(
    ILogger<CreateBrewerReviewController> logger,
    ICreateBrewerReviewService createBrewerReviewService)
    : ControllerBase
{
    private readonly ILogger<CreateBrewerReviewController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICreateBrewerReviewService _createBrewerReviewService = createBrewerReviewService ??
        throw new ArgumentNullException(nameof(createBrewerReviewService));

    /// <summary>
    /// Creates a new brewer review for a brewer.
    /// </summary>
    /// <param name="brewerId">The brewer unique identifier.</param>
    /// <param name="inputModel">The review payload to create.</param>
    /// <returns>The newly created brewer review.</returns>
    [HttpPost]
    [Produces(typeof(ReadBrewerReviewModel))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostAsync(
        Guid brewerId,
        [FromBody][Required] CreateBrewerReviewModel? inputModel)
    {
        if (inputModel == null || !ModelState.IsValid)
        {
            const string message = "Unable to create brewer review because of an invalid input model.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        inputModel.BrewerId = brewerId;

        var (model, errors) = await _createBrewerReviewService.CreateAsync(inputModel).ConfigureAwait(false);
        if (errors.Count != 0)
        {
            return BadRequest(errors.Select(x => x.ErrorMessage));
        }

        return CreatedAtRoute("GetBrewerReviewByIdAsync", new { brewerId = model.BrewerId, reviewId = model.ReviewId }, model);
    }
}

using Asp.Versioning;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Constants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Beers.API.Controllers.v1.BrewerReview;

/// <summary>
/// The controller that coordinates deleting Brewer Review information.
/// </summary>
/// <remarks>
/// The controller that coordinates deleting Brewer Review information.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "BrewerReview")]
[Route("api/v1/brewers/{brewerId:guid}/reviews", Name = "Delete Brewer Review Controller v1")]
[Produces("application/json")]
public sealed class DeleteBrewerReviewController(
    ILogger<DeleteBrewerReviewController> logger,
    IDeleteBrewerReviewService deleteBrewerReviewService)
    : ControllerBase
{
    private readonly ILogger<DeleteBrewerReviewController> _logger = logger ??
        throw new ArgumentNullException(nameof(logger));
    private readonly IDeleteBrewerReviewService _deleteBrewerReviewService = deleteBrewerReviewService ??
        throw new ArgumentNullException(nameof(deleteBrewerReviewService));

    /// <summary>
    /// Delete a single brewer review record.
    /// </summary>
    /// <param name="brewerId">Unique id of a brewer.</param>
    /// <param name="reviewId">Unique id of a review.</param>
    /// <returns>An http result.</returns>
    [HttpDelete("{reviewId:guid}")]
    public async Task<IActionResult> DeleteAsync([Required] Guid brewerId, [Required] Guid reviewId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Unable to delete brewer review because of an invalid input model.");
        }

        var (result, errors) = await _deleteBrewerReviewService.DeleteAsync(brewerId, reviewId);
        _logger.LogInformation("Result of deleting brewer review with id {ReviewId} was {Model}", reviewId, result);

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

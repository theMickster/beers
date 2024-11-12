using Asp.Versioning;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Constants;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Beers.API.Controllers.v1.Beer;

/// <summary>
/// The controller that coordinates deleting Beer information.
/// </summary>
/// <remarks>
/// The controller that coordinates deleting Beer information.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Beer")]
[Route("api/v1/beers", Name = "Delete Beer Controller v1")]
[Produces("application/json")]
public class DeleteBeerController(ILogger<DeleteBeerController> logger, IDeleteBeerService deleteBeerService) : ControllerBase
{
    private readonly ILogger<DeleteBeerController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDeleteBeerService _deleteBeerService = deleteBeerService ?? throw new ArgumentNullException(nameof(deleteBeerService));

    /// <summary>
    /// Delete a single beer record
    /// </summary>
    /// <param name="beerId">unique id of a beer</param>
    /// <returns></returns>
    [HttpDelete("{beerId:guid}")]
    public async Task<IActionResult> DeleteAsync([Required] Guid beerId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Unable to delete beer because of an invalid input model.");
        }

        var (result, errors) = await _deleteBeerService.DeleteAsync(beerId);

        _logger.LogInformation("Result of deleting beer with id {BeerId} was {Model}", beerId, result);

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

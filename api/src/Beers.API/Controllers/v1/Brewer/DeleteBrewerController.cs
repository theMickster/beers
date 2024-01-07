using Asp.Versioning;
using Beers.Application.Interfaces.Services;
using Beers.Domain.Models.Brewer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Beers.API.Controllers.v1.Brewer;

/// <summary>
/// The controller that coordinates deleting Brewer information.
/// </summary>
/// <remarks>
/// The controller that coordinates deleting Brewer information.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Brewer")]
[Route("api/v1/brewers", Name = "Delete Brewer Controller v1")]
[Produces("application/json")]
public class DeleteBrewerController(ILogger<DeleteBrewerController> logger, IDeleteBrewerService deleteBrewerService) 
    : ControllerBase
{
    private readonly ILogger<DeleteBrewerController> _logger = 
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDeleteBrewerService _deleteBrewerService = 
        deleteBrewerService ?? throw new ArgumentNullException(nameof(deleteBrewerService));

    /// <summary>
    /// Delete a single brewer record
    /// </summary>
    /// <param name="brewerId">unique id of a brewer</param>
    /// <returns></returns>
    [HttpDelete("{brewerId:guid}")]
    [Produces(typeof(ReadBrewerModel))]
    public async Task<IActionResult> DeleteAsync([Required] Guid brewerId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("The model state is invalid. Unable to delete the brewer");
        }

        var (model, errors) = await _deleteBrewerService.DeleteAsync(brewerId);

        if (errors.Count == 0)
        {
            return NoContent();
        }

        if (errors.Any(x => x.ErrorCode == "DeleteRule001"))
        {
            return NotFound();
        }

        return BadRequest(errors.Select(x => x.ErrorMessage));
    }

}

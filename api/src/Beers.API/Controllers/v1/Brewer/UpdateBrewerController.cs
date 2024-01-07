using Asp.Versioning;
using Beers.Application.Interfaces.Services;
using Beers.Domain.Models.Brewer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Beers.API.Controllers.v1.Brewer;

/// <summary>
/// The controller that coordinates updating Brewer information.
/// </summary>
/// <remarks>
/// The controller that coordinates updating Brewer information.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Brewer")]
[Route("api/v1/brewers", Name = "Update Brewer Controller v1")]
[Produces("application/json")]
public sealed class UpdateBrewerController(ILogger<UpdateBrewerController> logger, IUpdateBrewerService updateBrewerService) 
    : ControllerBase
{
    private readonly ILogger<UpdateBrewerController> _logger = 
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IUpdateBrewerService _updateBrewerService = 
        updateBrewerService ?? throw new ArgumentNullException(nameof(updateBrewerService));

    /// <summary>
    /// Update a single brewer record
    /// </summary>
    /// <param name="brewerId">unique id of a brewer</param>
    /// <param name="inputModel">brewer update model</param>
    /// <returns></returns>
    [HttpPut("{brewerId:guid}")]
    [Produces(typeof(ReadBrewerModel))]
    public async Task<IActionResult> PutAsync([Required]Guid brewerId, [FromBody][Required] UpdateBrewerModel? inputModel)
    {
        if (inputModel == null)
        {
            return BadRequest("The input model cannot be null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (brewerId != inputModel.BrewerId)
        {
            return BadRequest("The brewer id parameter must match the id of the brewer update request payload.");
        }

        var (model, errors) = await _updateBrewerService.UpdateAsync(inputModel);

        if (errors.Any())
        {
            return BadRequest(errors.Select(x => x.ErrorMessage));
        }

        return Ok(model);

    }
}

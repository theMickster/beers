using Asp.Versioning;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Domain.Models.Beer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace Beers.API.Controllers.v1.Beer;

/// <summary>
/// The controller that coordinates updating Beer information.
/// </summary>
/// <remarks>
/// The controller that coordinates updating Beer information.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Beer")]
[Route("api/v1/beers/{beerId:guid}", Name = "Update Beer Controller v1")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class UpdateBeerController (ILogger<UpdateBeerController> logger, IUpdateBeerService updateBeerService) : ControllerBase
{
    private readonly ILogger<UpdateBeerController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IUpdateBeerService _updateBeerService = updateBeerService ?? throw new ArgumentNullException(nameof(updateBeerService));

    /// <summary>
    /// Update a single beer record
    /// </summary>
    /// <param name="beerId">unique id of a beer</param>
    /// <param name="inputModel">beer update model</param>
    /// <returns></returns>
    [HttpPut]
    [Produces(typeof(ReadBeerModel))]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> PutAsync([Required] Guid beerId, [FromBody][Required] UpdateBeerModel? inputModel)
    {
        if (inputModel == null)
        {
            return BadRequest("The input model cannot be null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (beerId != inputModel.BeerId)
        {
            _logger.LogInformation("Mismatch detected between the beer id route parameter and the beer id in the request payload.");
            return BadRequest("The beer id parameter must match the id of the beer update request payload.");
        }

        var (model, errors) = await _updateBeerService.UpdateAsync(inputModel);

        if (errors.Count != 0)
        {
            return BadRequest(errors.Select(x => x.ErrorMessage));
        }

        return Ok(model);
    }
}

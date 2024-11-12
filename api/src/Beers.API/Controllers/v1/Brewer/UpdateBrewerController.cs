using Asp.Versioning;
using AutoMapper;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Domain.Models.Brewer;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

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
[Route("api/v1/brewers/{brewerId:guid}", Name = "Update Brewer Controller v1")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class UpdateBrewerController(
    ILogger<UpdateBrewerController> logger, 
    IUpdateBrewerService updateBrewerService,
    IReadBrewerService readBrewerService,
    IMapper mapper) 
    : ControllerBase
{
    private readonly ILogger<UpdateBrewerController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IUpdateBrewerService _updateBrewerService = updateBrewerService ?? throw new ArgumentNullException(nameof(updateBrewerService));
    private readonly IReadBrewerService _readBrewerService = readBrewerService ?? throw new ArgumentNullException(nameof(readBrewerService));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    /// <summary>
    /// Update a single brewer record
    /// </summary>
    /// <param name="brewerId">unique id of a brewer</param>
    /// <param name="inputModel">brewer update model</param>
    /// <returns></returns>
    [HttpPut]
    [Produces(typeof(ReadBrewerModel))]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ActionResult<ReadBrewerModel>> PutAsync([Required]Guid brewerId, [FromBody][Required] UpdateBrewerModel? inputModel)
    {
        if (inputModel == null || !ModelState.IsValid)
        {
            const string message = "Unable to update brewer because of an invalid input model.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        if (brewerId != inputModel.BrewerId)
        {
            const string message = "The brewer id parameter must match the id of the brewer update request payload.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        var (model, errors) = await _updateBrewerService.UpdateAsync(inputModel);

        if (errors.Count != 0)
        {
            return BadRequest(errors.Select(x => x.ErrorMessage));
        }

        return Ok(model);
    }

    /// <summary>
    /// Update a single brewer record via JSON Patch process
    /// </summary>
    /// <param name="brewerId">unique id of a brewer</param>
    /// <param name="inputModel">brewer update model</param>
    /// <returns></returns>
    [HttpPatch]
    [Produces(typeof(ReadBrewerModel))]
    [Consumes(MediaTypeNames.Application.JsonPatch)]
    public async Task<ActionResult<ReadBrewerModel>> PatchAsync([Required] Guid brewerId,
        [FromBody] JsonPatchDocument<UpdateBrewerModel> inputModel)
    {
        if (inputModel == null)
        {
            const string message = "Invalid patch document";
            _logger.LogInformation(message);
            return BadRequest(message);
        }
        
        var existingBrewer = await _readBrewerService.GetByIdAsync(brewerId);
        if (existingBrewer == null)
        {
            const string message = "Unable to locate model.";
            _logger.LogInformation(message);
            return NotFound();
        }

        var updateBrewer = _mapper.Map<UpdateBrewerModel>(existingBrewer);

        inputModel.ApplyTo(updateBrewer);
        
        var (model, errors) = await _updateBrewerService.UpdateAsync(updateBrewer);
        
        if (errors.Count != 0)
        {
            return BadRequest(errors.Select(x => x.ErrorMessage));
        }
        
        return Ok(model);
    }
}

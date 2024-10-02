using Asp.Versioning;
using Beers.Application.Interfaces.Services;
using Beers.Domain.Models.Brewer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Beers.API.Controllers.v1.Brewer;

/// <summary>
/// The controller that coordinates creating Brewer information.
/// </summary>
/// <remarks>
/// The controller that coordinates retrieving Brewer information.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Brewer")]
[Route("api/v1/brewers", Name = "Create Brewer Controller v1")]
[Produces("application/json")]
public class CreateBrewerController(ILogger<CreateBrewerController> logger, ICreateBrewerService createBrewerService) 
    : ControllerBase
{
    private readonly ILogger<CreateBrewerController> _logger = 
        logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICreateBrewerService _createBrewerService = 
        createBrewerService ?? throw new ArgumentNullException(nameof(createBrewerService));

    /// <summary>
    /// Creates a new brewer
    /// </summary>
    /// <param name="inputModel">the brewer to create</param>
    /// <returns></returns>
    [HttpPost]
    [Produces(typeof(ReadBrewerModel))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostAsync([FromBody][Required] CreateBrewerModel? inputModel)
    {
        if (inputModel == null)
        {
            _logger.LogInformation("Create Brewer failed due to null input model.");
            return BadRequest("The input model cannot be null.");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogInformation("Create Brewer failed due to invalid input model state.");
            return BadRequest(ModelState);
        }

        var (model, errors) = await _createBrewerService.CreateAsync(inputModel).ConfigureAwait(false);

        if (errors.Count != 0)
        {
            return BadRequest(errors.Select(x => x.ErrorMessage));
        }

        return CreatedAtRoute("GetBrewerById", new { model.BrewerId }, model);

    }
}

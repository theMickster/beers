using Asp.Versioning;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Domain.Models.Beer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Beers.API.Controllers.v1.Beer;

/// <summary>
/// The controller that coordinates creating Beer information.
/// </summary>
/// <remarks>
/// The controller that coordinates creating Beer information.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Beer")]
[Route("api/v1/beers", Name = "Create Beer Controller v1")]
[Produces("application/json")]
public class CreateBeerController(ILogger<CreateBeerController> logger, 
    ICreateBeerService createBeerService) : ControllerBase
{
    private readonly ILogger<CreateBeerController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ICreateBeerService _createBeerService = createBeerService ?? throw new ArgumentNullException(nameof(createBeerService));

    /// <summary>
    /// Creates a new beer
    /// </summary>
    /// <param name="inputModel">the beer to create</param>
    /// <returns></returns>
    [HttpPost]
    [Produces(typeof(ReadBeerModel))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PostAsync([FromBody][Required] CreateBeerModel? inputModel)
    {
        if (inputModel == null || !ModelState.IsValid)
        {
            const string message = "Unable to create beer because of an invalid input model.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        var (model, errors) = await _createBeerService.CreateAsync(inputModel).ConfigureAwait(false);

        if (errors.Count != 0)
        {
            return BadRequest(errors.Select(x => x.ErrorMessage));
        }

        
        return CreatedAtRoute(nameof(ReadBeerController.GetBeerByIdAsync), new { model.BeerId }, model);
    }
}

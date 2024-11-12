using Asp.Versioning;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Domain.Models.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace Beers.API.Controllers.v1.BreweryType;

/// <summary>
/// The controller that coordinates retrieving Brewery Type information.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Brewery Type")]
[Route("api/v1/breweryTypes", Name = "Read Brewery Type Controller v1")]
[Produces("application/json")]
public class ReadBreweryTypeController(ILogger<ReadBreweryTypeController> logger,
    IReadBreweryTypeService readBreweryTypeService) : ControllerBase
{
    private readonly IReadBreweryTypeService _readBreweryTypeService = readBreweryTypeService ?? throw new ArgumentNullException(nameof(readBreweryTypeService));
    private readonly ILogger<ReadBreweryTypeController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Retrieve the list of brewery types
    /// </summary>
    /// <returns>A list of brewery types</returns>
    /// <response code="200">Returns the list of brewery types</response>
    /// <response code="404">If the list of brewery types cannot be found</response>
    [HttpGet]
    [Produces(typeof(List<BreweryTypeModel>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _readBreweryTypeService.GetListAsync<BreweryTypeModel>();

        if (model.Count == 0)
        {
            const string message = "Unable to locate records the brewery type list.";
            _logger.LogInformation(message);
            return NotFound(message);
        }

        return Ok(model);
    }
}

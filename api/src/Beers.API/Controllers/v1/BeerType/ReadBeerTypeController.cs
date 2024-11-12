using Asp.Versioning;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Domain.Models.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace Beers.API.Controllers.v1.BeerType;

/// <summary>
/// The controller that coordinates retrieving Beer Type information.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Beer Type")]
[Route("api/v1/beerTypes", Name = "Read Beer Type Controller v1")]
[Produces("application/json")]
public sealed class ReadBeerTypeController(ILogger<ReadBeerTypeController> logger,
    IReadBeerTypeService readBeerTypeService) : ControllerBase
{
    private readonly IReadBeerTypeService _readBeerTypeService = readBeerTypeService ?? throw new ArgumentNullException(nameof(readBeerTypeService));
    private readonly ILogger<ReadBeerTypeController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Retrieve the list of beer types
    /// </summary>
    /// <returns>A list of beer types</returns>
    /// <response code="200">Returns the list of beer types</response>
    /// <response code="404">If the list of beer types cannot be found</response>
    [HttpGet]
    [Produces(typeof(List<BeerTypeModel>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _readBeerTypeService.GetListAsync<BeerTypeModel>();

        if (model.Count == 0)
        {
            const string message = "Unable to locate records the beer type list.";
            _logger.LogInformation(message);
            return NotFound(message);
        }

        return Ok(model);
    }
}
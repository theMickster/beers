using Asp.Versioning;
using Beers.Application.Interfaces.Services;
using Beers.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Beers.API.Controllers.v1.BeerType;

/// <summary>
/// The controller that coordinates retrieving Beer Type information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Beer Type")]
[Route("api/v1/beerTypes", Name = "Read Beer Type Controller v1")]
[Produces("application/json")]
public sealed class ReadBeerTypeController : ControllerBase
{
    private readonly IReadBeerTypeService _readBeerTypeService;
    private readonly ILogger<ReadBeerTypeController> _logger;

    /// <summary>
    /// The controller that coordinates retrieving Beer Type information.
    /// </summary>
    public ReadBeerTypeController(ILogger<ReadBeerTypeController> logger, IReadBeerTypeService readBeerTypeService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readBeerTypeService = readBeerTypeService ?? throw new ArgumentNullException(nameof(readBeerTypeService));
    }

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
    public IActionResult GetList()
    {
        var model = _readBeerTypeService.GetList();

        if (!model.Any())
        {
            return NotFound("Unable to locate records the beer type list.");
        }

        return Ok(model);
    }
}
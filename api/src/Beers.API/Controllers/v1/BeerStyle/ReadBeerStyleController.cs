using Asp.Versioning;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Domain.Models.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace Beers.API.Controllers.v1.BeerStyle;

/// <summary>
/// The controller that coordinates retrieving Beer Style information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Beer Style")]
[Route("api/v1/beerStyles", Name = "Read Beer Style Controller v1")]
[Produces("application/json")]
public sealed class ReadBeerStyleController : ControllerBase
{
    private readonly IReadBeerStyleService _readBeerStyleService;
    private readonly ILogger<ReadBeerStyleController> _logger;

    /// <summary>
    /// The controller that coordinates retrieving Beer Style information.
    /// </summary>
    public ReadBeerStyleController(ILogger<ReadBeerStyleController> logger, IReadBeerStyleService readBeerStyleService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readBeerStyleService = readBeerStyleService ?? throw new ArgumentNullException(nameof(readBeerStyleService));
    }

    /// <summary>
    /// Retrieve the list of beer styles
    /// </summary>
    /// <returns>A list of beer styles</returns>
    /// <response code="200">Returns the list of beer styles</response>
    /// <response code="404">If the list of beer styles cannot be found</response>
    [HttpGet]
    [Produces(typeof(List<BeerStyleModel>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _readBeerStyleService.GetListAsync<BeerStyleModel>();

        if (model.Count == 0)
        {
            return NotFound("Unable to locate records the beer style list.");
        }

        return Ok(model);
    }
}

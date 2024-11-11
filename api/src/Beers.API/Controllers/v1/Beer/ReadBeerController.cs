using Asp.Versioning;
using Beers.Application.Interfaces.Services.Beer;
using Beers.Common.Filtering.Beer;
using Beers.Domain.Models.Beer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Beers.API.Controllers.v1.Beer;

/// <summary>
/// The controller that coordinates retrieving Beer information.
/// </summary>
/// <remarks>
/// The controller that coordinates retrieving Beer information.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Beer")]
[Route("api/v1/beers", Name = "Read Beer Controller v1")]
[Produces("application/json")]
public sealed class ReadBeerController(ILogger<ReadBeerController> logger, IReadBeerService readBeerService)
    : ControllerBase
{
    private readonly ILogger<ReadBeerController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IReadBeerService _readBeerService = readBeerService ?? throw new ArgumentNullException(nameof(readBeerService));

    /// <summary>
    /// Retrieve the list of beers
    /// </summary>
    /// <returns>A list of beers</returns>
    /// <response code="200">Returns the list of beers</response>
    /// <response code="404">If the list of beers cannot be found</response>
    [HttpGet]
    [Produces(typeof(List<ReadBeerModel>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ReadBeerModel>>> GetListAsync()
    {
        var model = await _readBeerService.GetListAsync();
        
        if (model.Count == 0)
        {
            const string message = "Unable to locate records for the beers list.";
            _logger.LogInformation(message);
            return NotFound();
        }
        
        return Ok(model);
    }

    /// <summary>
    /// Retrieve a paged list of beers
    /// </summary>
    /// <returns>A list of beers</returns>
    /// <response code="200">Returns the list of beers</response>
    /// <response code="404">If the list of beers cannot be found</response>
    [HttpPost]
    [Route("search", Name = "SearchBeersAsync")]
    [Produces(typeof(SearchResultBeerModel))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SearchResultBeerModel>> SearchAsync(
        [FromQuery] SearchBeerParameter parameters,
        [FromBody][Required] SearchInputBeerModel searchModel )
    {
        if (!ModelState.IsValid)
        {
            const string message = "Unable to search for beers because of an invalid input model.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }
        
        var result = await _readBeerService.SearchAsync(parameters, searchModel);
        
        return Ok(result);
    }

    /// <summary>
    /// Retrieve a beer using its unique identifier
    /// </summary>
    /// <param name="beerId">the unique identifier</param>
    /// <returns>A single beer</returns>
    [HttpGet("{beerId:guid}", Name = "GetByIdAsync")]
    [Produces(typeof(ReadBeerModel))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadBeerModel>> GetByIdAsync(Guid beerId)
    {
        var model = await _readBeerService.GetByIdAsync(beerId);
        if (model == null)
        {
            const string message = "Unable to locate beer model.";
            _logger.LogInformation(message);
            return NotFound();
        }
        return Ok(model);
    }
    
}

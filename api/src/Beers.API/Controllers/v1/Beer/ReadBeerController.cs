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
    public async Task<ActionResult<IReadOnlyList<ReadBeerModel>>> GetBeersAsync()
    {
        var model = await readBeerService.GetListAsync();
        
        if (!model.Any())
        {
            return NotFound("Unable to locate records for the beers list.");
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
    public async Task<ActionResult<SearchResultBeerModel>> SearchBeersAsync(
        [FromQuery] SearchBeerParameter parameters,
        [FromBody][Required] SearchInputBeerModel searchModel )
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await readBeerService.SearchAsync(parameters, searchModel);
        
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
        var model = await readBeerService.GetByIdAsync(beerId);
        if (model == null)
        {
            return NotFound("Unable to locate beer model.");
        }
        return Ok(model);
    }
    
}

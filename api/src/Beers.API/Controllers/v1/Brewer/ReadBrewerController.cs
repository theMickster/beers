using Asp.Versioning;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Domain.Models.Brewer;
using Microsoft.AspNetCore.Mvc;

namespace Beers.API.Controllers.v1.Brewer;

/// <summary>
/// The controller that coordinates retrieving Brewer information.
/// </summary>
/// <remarks>
/// The controller that coordinates retrieving Brewer information.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Brewer")]
[Route("api/v1/brewers", Name = "Read Brewer Controller v1")]
[Produces("application/json")]
public sealed class ReadBrewerController(ILogger<ReadBrewerController> logger, IReadBrewerService readBrewerService) 
    : ControllerBase
{
    private readonly ILogger<ReadBrewerController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IReadBrewerService _readBrewerService = readBrewerService ?? throw new ArgumentNullException(nameof(readBrewerService));

    /// <summary>
    /// Retrieve the list of brewers
    /// </summary>
    /// <returns>A list of brewers</returns>
    /// <response code="200">Returns the list of brewers</response>
    /// <response code="404">If the list of brewers cannot be found</response>
    [HttpGet]
    [Produces(typeof(List<ReadBrewerModel>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ReadBrewerModel>>> GetListAsync()
    {
        var model = await _readBrewerService.GetListAsync();

        if (!model.Any())
        {
            const string message = "Unable to locate records for the brewers list.";
            _logger.LogInformation(message);
            return NotFound();
        }

        return Ok(model);
    }

    /// <summary>
    /// Retrieve an brewer using its unique identifier
    /// </summary>
    /// <param name="brewerId">the unique identifier</param>
    /// <returns>A single brewer</returns>
    [HttpGet("{brewerId:guid}", Name = "GetBrewerByIdAsync")]
    [Produces(typeof(ReadBrewerModel))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadBrewerModel>> GetBrewerByIdAsync(Guid brewerId)
    {
        var brewer = await _readBrewerService.GetByIdAsync(brewerId);

        if (brewer == null)
        {
            const string message = "Unable to locate model.";
            _logger.LogInformation(message);
            return NotFound();
        }

        return Ok(brewer);
    }
}

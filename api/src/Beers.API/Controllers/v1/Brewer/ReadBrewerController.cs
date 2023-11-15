using Asp.Versioning;
using Beers.Application.Interfaces.Services;
using Beers.Domain.Models;
using Beers.Domain.Models.Brewer;
using Microsoft.AspNetCore.Mvc;

namespace Beers.API.Controllers.v1.Brewer;

/// <summary>
/// The controller that coordinates retrieving Brewer information.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Brewer")]
[Route("api/v1/brewers", Name = "Read Brewer Controller v1")]
[Produces("application/json")]
public sealed class ReadBrewerController : ControllerBase
{
    private readonly ILogger<ReadBrewerController> _logger;
    private readonly IReadBrewerService _readBrewerService;

    /// <summary>
    /// The controller that coordinates retrieving Brewer information.
    /// </summary>
    public ReadBrewerController(ILogger<ReadBrewerController> logger, IReadBrewerService readBrewerService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readBrewerService = readBrewerService ?? throw new ArgumentNullException(nameof(readBrewerService));
    }

    /// <summary>
    /// Retrieve the list of brewers
    /// </summary>
    /// <returns>A list of brewers</returns>
    /// <response code="200">Returns the list of brewers</response>
    /// <response code="404">If the list of brewers cannot be found</response>
    [HttpGet]
    [Produces(typeof(List<ReadBrewerModel>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _readBrewerService.GetListAsync();

        if (!model.Any())
        {
            return NotFound("Unable to locate records for the brewers list.");
        }

        return Ok(model);
    }
}

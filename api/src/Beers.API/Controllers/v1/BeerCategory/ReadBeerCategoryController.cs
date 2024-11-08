using Asp.Versioning;
using Beers.Application.Interfaces.Services.Metadata;
using Beers.Domain.Models.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace Beers.API.Controllers.v1.BeerCategory;

/// <summary>
/// The controller that coordinates retrieving Beer Category information.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Beer Category")]
[Route("api/v1/beerCategories", Name = "Read Beer Category Controller v1")]
[Produces("application/json")]
public sealed class ReadBeerCategoryController(ILogger<ReadBeerCategoryController> logger, 
    IReadBeerCategoryService readBeerCategoryService) : ControllerBase
{
    private readonly IReadBeerCategoryService _readBeerCategoryService =
        readBeerCategoryService ?? throw new ArgumentNullException(nameof(readBeerCategoryService));
    private readonly ILogger<ReadBeerCategoryController> _logger = 
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Retrieve the list of beer categories
    /// </summary>
    /// <returns>A list of beer categories</returns>
    /// <response code="200">Returns the list of beer categories</response>
    /// <response code="404">If the list of beer categories cannot be found</response>
    [HttpGet]
    [Produces(typeof(List<BeerCategoryModel>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListAsync()
    {
        var model = await _readBeerCategoryService.GetListAsync<BeerCategoryModel>();

        if (model.Count == 0)
        {
            const string message = "Unable to locate records the beer category list.";
            _logger.LogInformation(message);
            return NotFound(message);
        }

        return Ok(model);
    }
}

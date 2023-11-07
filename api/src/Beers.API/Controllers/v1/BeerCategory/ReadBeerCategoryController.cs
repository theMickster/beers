using Asp.Versioning;
using Beers.Application.Interfaces.Services;
using Beers.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Beers.API.Controllers.v1.BeerCategory;

/// <summary>
/// The controller that coordinates retrieving Beer Category information.
/// </summary>
/// <remarks></remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Beer Category")]
[Route("api/v1/beerCategories", Name = "Read Beer Category Controller v1")]
[Produces("application/json")]
public sealed class ReadBeerCategoryController : ControllerBase
{
    private readonly IReadBeerCategoryService _readBeerCategoryService;
    private readonly ILogger<ReadBeerCategoryController> _logger;

    /// <summary>
    /// The controller that coordinates retrieving Beer Category information.
    /// </summary>
    public ReadBeerCategoryController(ILogger<ReadBeerCategoryController> logger, IReadBeerCategoryService readBeerCategoryService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readBeerCategoryService = readBeerCategoryService ?? throw new ArgumentNullException(nameof(readBeerCategoryService));
    }

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
    public IActionResult GetList()
    {
        var model = _readBeerCategoryService.GetList();

        if (!model.Any())
        {
            return NotFound("Unable to locate records the beer category list.");
        }

        return Ok(model);
    }
}

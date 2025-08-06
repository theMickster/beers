using Asp.Versioning;
using Beers.Application.Interfaces.Services.Brewer;
using Beers.Common.Filtering.BrewerReview;
using Beers.Domain.Models.Brewer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Beers.API.Controllers.v1.BrewerReview;

/// <summary>
/// The controller that coordinates retrieving Brewer Review information.
/// </summary>
/// <remarks>
/// The controller that coordinates retrieving Brewer Review information.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "BrewerReview")]
[Route("api/v1/brewers/{brewerId:guid}/reviews", Name = "Read Brewer Review Controller v1")]
[Produces("application/json")]
public sealed class ReadBrewerReviewController(
    ILogger<ReadBrewerReviewController> logger,
    IReadBrewerReviewService readBrewerReviewService)
    : ControllerBase
{
    private readonly ILogger<ReadBrewerReviewController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IReadBrewerReviewService _readBrewerReviewService = readBrewerReviewService ??
        throw new ArgumentNullException(nameof(readBrewerReviewService));

    /// <summary>
    /// Retrieve the list of reviews for a brewer.
    /// </summary>
    /// <param name="brewerId">The brewer unique identifier.</param>
    /// <returns>A list of brewer reviews.</returns>
    [HttpGet]
    [Produces(typeof(List<ReadBrewerReviewModel>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ReadBrewerReviewModel>>> GetListAsync(Guid brewerId)
    {
        var model = await _readBrewerReviewService.GetListAsync(brewerId);
        if (model.Count == 0)
        {
            const string message = "Unable to locate records for the brewer review list.";
            _logger.LogInformation(message);
            return NotFound();
        }

        return Ok(model);
    }

    /// <summary>
    /// Retrieve a brewer review using its unique identifier.
    /// </summary>
    /// <param name="brewerId">The brewer unique identifier.</param>
    /// <param name="reviewId">The review unique identifier.</param>
    /// <returns>A single brewer review.</returns>
    [HttpGet("{reviewId:guid}", Name = "GetBrewerReviewByIdAsync")]
    [Produces(typeof(ReadBrewerReviewModel))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReadBrewerReviewModel>> GetBrewerReviewByIdAsync(Guid brewerId, Guid reviewId)
    {
        var model = await _readBrewerReviewService.GetByIdAsync(brewerId, reviewId);
        if (model == null)
        {
            const string message = "Unable to locate brewer review model.";
            _logger.LogInformation(message);
            return NotFound();
        }

        return Ok(model);
    }

    /// <summary>
    /// Retrieve a paged list of brewer reviews using optional filters.
    /// </summary>
    /// <param name="brewerId">The brewer unique identifier from route.</param>
    /// <param name="parameters">Search paging and ordering parameters.</param>
    /// <param name="searchModel">Search filters.</param>
    /// <returns>A paged search result of brewer reviews.</returns>
    [HttpPost]
    [Route("search", Name = "SearchBrewerReviewsAsync")]
    [Produces(typeof(SearchResultBrewerReviewModel))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchResultBrewerReviewModel>> SearchAsync(
        Guid brewerId,
        [FromQuery] SearchBrewerReviewParameter parameters,
        [FromBody][Required] SearchInputBrewerReviewModel searchModel)
    {
        if (!ModelState.IsValid)
        {
            const string message = "Unable to search for brewer reviews because of an invalid input model.";
            _logger.LogInformation(message);
            return BadRequest(message);
        }

        searchModel.BrewerId = brewerId;
        var result = await _readBrewerReviewService.SearchAsync(parameters, searchModel);
        return Ok(result);
    }
}

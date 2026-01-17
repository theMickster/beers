namespace Beers.Common.Filtering.Beer;

/// <summary>
/// The filters applied when discovering beers. Values within one list are OR'd; different filters are AND'd.
/// </summary>
public sealed class FacetedSearchInputModel
{
    public List<Guid> BeerStyleIds { get; set; } = [];

    public List<Guid> BeerCategoryIds { get; set; } = [];

    public List<Guid> BrewerIds { get; set; } = [];

    public List<Guid> BeerTypeIds { get; set; } = [];

    public decimal? MinRating { get; set; }

    public decimal? MaxRating { get; set; }

    public string? Name { get; set; }
}

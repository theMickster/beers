using Beers.Common.Filtering.Base;

namespace Beers.Common.Filtering.Beer;
public sealed class SearchInputBeerModel : SearchInputModelBase
{
    public string BrewerName { get; set; } = string.Empty;

    public Guid? BrewerId { get; set; }

    public List<Guid> BeerStyleIds { get; set; } = [];

    public List<Guid> BeerCategoryIds { get; set; } = [];

    public List<Guid> BeerTypeIds { get; set; } = [];

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }
}

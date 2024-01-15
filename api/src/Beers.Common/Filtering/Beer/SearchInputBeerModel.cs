using Beers.Common.Filtering.Base;

namespace Beers.Common.Filtering.Beer;
public sealed class SearchInputBeerModel : SearchInputModelBase
{
    public string BrewerName { get; set; } = string.Empty;

    public Guid? BrewerId { get; set; }
}

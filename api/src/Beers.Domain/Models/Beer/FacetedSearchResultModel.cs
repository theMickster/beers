using Beers.Domain.Models.Base;

namespace Beers.Domain.Models.Beer;

public sealed class FacetedSearchResultModel : SearchResultBaseModel<ReadBeerModel>
{
    public BeerFacetsModel Facets { get; set; } = new();
}

public sealed class BeerFacetsModel
{
    public List<FacetItemModel> StyleFacets { get; set; } = [];

    public List<FacetItemModel> CategoryFacets { get; set; } = [];

    public List<FacetItemModel> BrewerFacets { get; set; } = [];

    public List<FacetItemModel> BeerTypeFacets { get; set; } = [];
}

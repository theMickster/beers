using Beers.Domain.Models.Beer;
using Beers.Domain.Models.Metadata;

namespace Beers.Domain.Models.Base;

public abstract class BaseBeerModel
{
    public Guid BrewerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public BrewerModel Brewer { get; set; }

    public RatingModel Rating { get; set; }

    public List<PriceModel>? Pricing { get; set; }
    
    public BeerTypeModel BeerType { get; set; }

    public List<BeerCategoryModel> BeerCategories { get; set; }

    public List<BeerStyleModel> BeerStyles { get; set; }

}

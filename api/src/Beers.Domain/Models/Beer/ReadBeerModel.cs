using Beers.Domain.Models.Base;
using Beers.Domain.Models.Metadata;

namespace Beers.Domain.Models.Beer;

public class ReadBeerModel: BaseBeerModel
{
    public Guid BeerId { get; set; }
    
    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }

    public BrewerModel Brewer { get; set; } = new();

    public BeerTypeModel BeerType { get; set; } = new();

    public List<BeerCategoryModel> BeerCategories { get; set; } = [];

    public List<BeerStyleModel> BeerStyles { get; set; } = [];
}

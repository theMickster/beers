using Beers.Common.Constants;
using Beers.Domain.Entities.Base;
using Beers.Domain.Entities.Slims;

namespace Beers.Domain.Entities;

public class BeerEntity : BaseBeerEntity
{
    public string EntityType = PartitionKeyConstants.Beer;
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public BrewerSlimEntity Brewer { get; set; }
    
    //public RatingEntity Rating { get; set; }
    
    //public BeerTypeSlimEntity BeerType { get; set; }

    //public List<BeerStyleSlimEntity> BeerStyles { get; set; }

    //public List<BeerCategorySlimEntity> BeerCategories { get; set; }

    public List<PriceEntity> Pricing { get; set; }
    
    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }
}

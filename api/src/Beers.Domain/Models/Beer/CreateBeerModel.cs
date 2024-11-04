using Beers.Domain.Models.Base;

namespace Beers.Domain.Models.Beer;

public class CreateBeerModel : BaseBeerModel
{
    public Guid BeerTypeId { get; set; }

    public List<Guid> BeerCategories { get; set; } = [];

    public List<Guid> BeerStyles { get; set; } = [];
}

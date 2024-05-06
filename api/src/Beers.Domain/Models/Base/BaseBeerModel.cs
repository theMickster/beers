using Beers.Domain.Models.Beer;

namespace Beers.Domain.Models.Base;

public abstract class BaseBeerModel
{
    public Guid BrewerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public RatingModel? Rating { get; set; }

    public List<PriceModel>? Pricing { get; set; }
}

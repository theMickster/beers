using Beers.Domain.Models.Base;

namespace Beers.Domain.Models.Brewer;

public sealed class UpdateBrewerModel : BaseBrewerModel
{
    public Guid BrewerId { get; set; }
}

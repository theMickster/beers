using Beers.Domain.Models.Base;

namespace Beers.Domain.Models.Brewer;

public sealed class ReadBrewerModel : BaseBrewerModel
{
    public Guid BrewerId { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ModifiedDate { get; set; }
}
